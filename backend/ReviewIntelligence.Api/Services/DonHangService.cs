using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.DonHang;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Models.Entities;
using DonHangEntity = ReviewIntelligence.Api.Models.Entities.DonHang;

namespace ReviewIntelligence.Api.Services;

public interface IDonHangService
{
    Task<KetQua<DonHangChiTietDto>> DatHangAsync(int maNguoiDung, DatHangRequest req, CancellationToken ct);
    Task<KetQuaPhanTrang<DonHangTomTatDto>> DanhSachCuaToiAsync(int maNguoiDung, PhanTrangQuery q, CancellationToken ct);
    Task<KetQua<DonHangChiTietDto>> ChiTietCuaToiAsync(int maNguoiDung, int maDonHang, CancellationToken ct);
    Task<KetQua<DonHangChiTietDto>> HuyAsync(int maNguoiDung, int maDonHang, CancellationToken ct);
    Task<DanhSachDonHangQuanTriDto> DanhSachQuanTriAsync(DonHangQuanTriQuery q, CancellationToken ct);
    Task<KetQua<DonHangChiTietDto>> DoiTrangThaiQuanTriAsync(int maQuanTri, int maDonHang, DoiTrangThaiDonHangRequest req, CancellationToken ct);
}

public class DonHangService(AppDbContext db, ILogger<DonHangService> logger) : IDonHangService
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    // =================================================================== Đặt hàng

    public async Task<KetQua<DonHangChiTietDto>> DatHangAsync(int maNguoiDung, DatHangRequest req, CancellationToken ct)
    {
        // EnableRetryOnFailure không cho mở transaction thủ công trực tiếp → bọc trong execution strategy;
        // khi retry, toàn bộ khối chạy lại từ đầu với change tracker sạch.
        var strategy = db.Database.CreateExecutionStrategy();
        var kq = await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            var coTransactionNgoai = db.Database.CurrentTransaction is not null; // dùng khi kiểm thử với transaction rollback
            await using var tx = coTransactionNgoai ? null : await db.Database.BeginTransactionAsync(ct);

            var ketQua = await DatHangTrongTransactionAsync(maNguoiDung, req, ct);
            if (ketQua.OK)
            {
                if (tx is not null) await tx.CommitAsync(ct);
            }
            else
            {
                db.ChangeTracker.Clear();
                if (tx is not null) await tx.RollbackAsync(ct);
            }
            return ketQua;
        });

        if (!kq.OK) return KetQua<DonHangChiTietDto>.Loi(kq.MaLoi, kq.ThongBao!);
        logger.LogInformation("NguoiDung#{MaNguoiDung} đặt DonHang#{MaDonHang}.", maNguoiDung, kq.DuLieu);
        return await ChiTietCuaToiAsync(maNguoiDung, kq.DuLieu, ct);
    }

    /// <summary>
    /// Mọi kiểm tra diễn ra TRƯỚC SaveChanges nên trả lỗi thì không có gì được ghi.
    /// Khóa UPDLOCK theo thứ tự BienThe (MaBienThe tăng dần) → LoHang → PhieuGiamGia để tránh deadlock
    /// và chống bán vượt tồn kho khi nhiều người đặt cùng lúc.
    /// </summary>
    private async Task<KetQua<int>> DatHangTrongTransactionAsync(int maNguoiDung, DatHangRequest req, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var homNay = now.Date;

        var gio = await db.GioHang.Where(g => g.MaNguoiDung == maNguoiDung).OrderBy(g => g.MaBienThe).ToListAsync(ct);
        if (gio.Count == 0)
            return KetQua<int>.Loi(StatusCodes.Status400BadRequest, "Giỏ hàng trống.");

        var chiTiet = new List<ChiTietDonHang>();
        decimal tienHang = 0;

        foreach (var dong in gio)
        {
            var bt = await db.BienThe
                .FromSql($"SELECT * FROM dbo.BienThe WITH (UPDLOCK, ROWLOCK) WHERE MaBienThe = {dong.MaBienThe}")
                .Include(b => b.SanPham)
                .FirstOrDefaultAsync(ct);

            if (bt is null || !bt.DangHoatDong || bt.SanPham.TrangThai != TrangThaiSanPham.DangBan)
                return KetQua<int>.Loi(StatusCodes.Status400BadRequest,
                    $"Sản phẩm \"{bt?.SanPham.Ten ?? $"#{dong.MaBienThe}"}\" không còn kinh doanh, vui lòng xóa khỏi giỏ hàng.");

            var tenHienThi = $"{bt.SanPham.Ten} ({bt.DungTich})";
            if (bt.SoLuongTon < dong.SoLuong)
                return KetQua<int>.Loi(StatusCodes.Status409Conflict,
                    bt.SoLuongTon == 0 ? $"{tenHienThi} đã hết hàng." : $"{tenHienThi} chỉ còn {bt.SoLuongTon} sản phẩm.");

            // FEFO: lô còn hàng, chưa hết hạn, hạn sử dụng gần nhất xuất trước.
            var cacLo = await db.LoHang
                .FromSql($"SELECT * FROM dbo.LoHang WITH (UPDLOCK, ROWLOCK) WHERE MaBienThe = {bt.MaBienThe} AND SoLuongConLai > 0 AND HanSuDung >= {homNay}")
                .OrderBy(l => l.HanSuDung).ThenBy(l => l.MaLo)
                .ToListAsync(ct);

            var donGia = GiaBan.ApDung(bt.GiaBan, bt.GiaKhuyenMai, bt.KhuyenMaiTu, bt.KhuyenMaiDen, now);
            var conLai = dong.SoLuong;
            foreach (var lo in cacLo)
            {
                if (conLai == 0) break;
                var lay = Math.Min(lo.SoLuongConLai, conLai);
                lo.SoLuongConLai -= lay;
                conLai -= lay;
                // Mỗi lô một dòng ChiTietDonHang (MaLo) để sp_DoiTrangThaiDonHang hoàn đúng lô khi hủy.
                chiTiet.Add(new ChiTietDonHang
                {
                    MaBienThe = bt.MaBienThe,
                    MaLo = lo.MaLo,
                    DonGia = donGia,
                    SoLuong = lay,
                    ThanhTien = donGia * lay
                });
            }

            if (conLai > 0)
                return KetQua<int>.Loi(StatusCodes.Status409Conflict,
                    $"{tenHienThi} không đủ hàng trong các lô còn hạn sử dụng.");

            bt.SoLuongTon -= dong.SoLuong;
            tienHang += donGia * dong.SoLuong;
        }

        // Mã giảm giá: khóa dòng phiếu, kiểm tra lại toàn bộ điều kiện với TienHang thực tế, trừ lượt.
        PhieuGiamGia? phieu = null;
        decimal tienGiam = 0;
        if (!string.IsNullOrWhiteSpace(req.MaCode))
        {
            var maCode = req.MaCode.Trim();
            phieu = await db.PhieuGiamGia
                .FromSql($"SELECT * FROM dbo.PhieuGiamGia WITH (UPDLOCK, ROWLOCK) WHERE MaCode = {maCode}")
                .FirstOrDefaultAsync(ct);
            if (phieu is null)
                return KetQua<int>.Loi(StatusCodes.Status400BadRequest, "Mã giảm giá không tồn tại.");

            var (giam, loi) = PhieuGiamGiaService.TinhTienGiam(phieu, tienHang, now);
            if (loi is not null)
                return KetQua<int>.Loi(StatusCodes.Status400BadRequest, loi);

            tienGiam = giam;
            if (phieu.SoLuotConLai is not null) phieu.SoLuotConLai--;
        }

        var phiVanChuyen = BangPhiVanChuyen.Tinh(req.PhuongThucGiaoHang);
        var donHang = new DonHangEntity
        {
            MaNguoiDung = maNguoiDung,
            MaPhieu = phieu?.MaPhieu,
            TenNguoiNhan = req.TenNguoiNhan.Trim(),
            SoDienThoai = req.SoDienThoai.Trim(),
            DiaChiGiaoHang = req.DiaChiGiaoHang.Trim(),
            PhuongThucGiaoHang = req.PhuongThucGiaoHang,
            PhuongThucThanhToan = req.PhuongThucThanhToan,
            TienHang = tienHang,
            TienGiam = tienGiam,
            PhiVanChuyen = phiVanChuyen,
            TongTien = tienHang - tienGiam + phiVanChuyen, // CK_DonHang_1
            TrangThai = TrangThaiDonHang.ChoXacNhan,
            DaThanhToan = false,
            ChiTietDonHangs = chiTiet
        };
        db.DonHang.Add(donHang);
        db.GioHang.RemoveRange(gio);
        await db.SaveChangesAsync(ct);

        db.ThongBao.Add(new ThongBao
        {
            MaNguoiDung = maNguoiDung,
            NoiDung = $"Đặt hàng thành công đơn #{donHang.MaDonHang} – tổng {donHang.TongTien.ToString("N0", Vi)}đ. Đơn đang chờ xác nhận.",
            LienKet = $"/don-hang/{donHang.MaDonHang}"
        });
        await db.SaveChangesAsync(ct);

        return KetQua<int>.ThanhCong(donHang.MaDonHang);
    }

    // =================================================================== Khách hàng

    public async Task<KetQuaPhanTrang<DonHangTomTatDto>> DanhSachCuaToiAsync(int maNguoiDung, PhanTrangQuery q, CancellationToken ct)
    {
        var query = db.DonHang.AsNoTracking().Where(d => d.MaNguoiDung == maNguoiDung);
        var tongSo = await query.CountAsync(ct);

        var trang = await query
            .OrderByDescending(d => d.NgayDat).ThenByDescending(d => d.MaDonHang)
            .Skip(q.BoQua).Take(q.KichThuoc)
            .Select(d => new
            {
                d.MaDonHang, d.NgayDat, d.TrangThai, d.DaThanhToan, d.TongTien,
                TongSoLuong = d.ChiTietDonHangs.Sum(c => c.SoLuong),
                SoMatHang = d.ChiTietDonHangs.Select(c => c.MaBienThe).Distinct().Count(),
                Dau = d.ChiTietDonHangs.OrderBy(c => c.MaChiTietDH)
                    .Select(c => new { c.BienThe.SanPham.Ten, c.BienThe.SanPham.DanhSachAnh })
                    .FirstOrDefault()
            })
            .ToListAsync(ct);

        var duLieu = trang.Select(d => new DonHangTomTatDto(
            d.MaDonHang, d.NgayDat, d.TrangThai, d.DaThanhToan, d.TongTien, d.TongSoLuong, d.SoMatHang,
            d.Dau?.Ten, JsonCot.DocMangChuoi(d.Dau?.DanhSachAnh).FirstOrDefault())).ToList();

        return new KetQuaPhanTrang<DonHangTomTatDto>(duLieu, tongSo, q.Trang, q.KichThuoc);
    }

    public async Task<KetQua<DonHangChiTietDto>> ChiTietCuaToiAsync(int maNguoiDung, int maDonHang, CancellationToken ct)
    {
        var ds = await TruyVanChiTietAsync(
            db.DonHang.Where(d => d.MaDonHang == maDonHang && d.MaNguoiDung == maNguoiDung), ct);
        // Đơn của người khác cũng trả 404 để không lộ sự tồn tại.
        return ds.Count == 0
            ? KetQua<DonHangChiTietDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy đơn hàng.")
            : KetQua<DonHangChiTietDto>.ThanhCong(ds[0]);
    }

    public async Task<KetQua<DonHangChiTietDto>> HuyAsync(int maNguoiDung, int maDonHang, CancellationToken ct)
    {
        var trangThai = await db.DonHang.AsNoTracking()
            .Where(d => d.MaDonHang == maDonHang && d.MaNguoiDung == maNguoiDung)
            .Select(d => d.TrangThai)
            .FirstOrDefaultAsync(ct);

        if (trangThai is null)
            return KetQua<DonHangChiTietDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy đơn hàng.");
        if (trangThai != TrangThaiDonHang.ChoXacNhan)
            return KetQua<DonHangChiTietDto>.Loi(StatusCodes.Status400BadRequest,
                "Chỉ có thể hủy đơn hàng đang chờ xác nhận. Vui lòng liên hệ cửa hàng để được hỗ trợ.");

        // Thủ tục tự hoàn kho LoHang/BienThe, tạo ThongBao, ghi NhatKyHeThong trong 1 transaction.
        await db.DoiTrangThaiDonHangAsync(maDonHang, TrangThaiDonHang.DaHuy, maNguoiDung, ct);
        return await ChiTietCuaToiAsync(maNguoiDung, maDonHang, ct);
    }

    // =================================================================== Quản trị

    public async Task<DanhSachDonHangQuanTriDto> DanhSachQuanTriAsync(DonHangQuanTriQuery q, CancellationToken ct)
    {
        var soLuong = await db.DonHang.AsNoTracking()
            .GroupBy(d => d.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.TrangThai, x => x.SoLuong, ct);
        foreach (var tt in TrangThaiDonHang.TatCa) soLuong.TryAdd(tt, 0);

        var query = db.DonHang.AsQueryable();
        if (q.TrangThai is not null) query = query.Where(d => d.TrangThai == q.TrangThai);

        var tongSo = await query.CountAsync(ct);
        // Đơn đang xử lý: cũ nhất trước (FIFO); đơn đã kết thúc: mới nhất trước.
        var dangXuLy = q.TrangThai is not null && TrangThaiDonHang.TiepTheo(q.TrangThai).Count > 0;
        query = dangXuLy
            ? query.OrderBy(d => d.NgayDat).ThenBy(d => d.MaDonHang)
            : query.OrderByDescending(d => d.NgayDat).ThenByDescending(d => d.MaDonHang);

        var duLieu = await TruyVanChiTietAsync(query.Skip(q.BoQua).Take(q.KichThuoc), ct);
        return new DanhSachDonHangQuanTriDto(
            new KetQuaPhanTrang<DonHangChiTietDto>(duLieu, tongSo, q.Trang, q.KichThuoc), soLuong);
    }

    public async Task<KetQua<DonHangChiTietDto>> DoiTrangThaiQuanTriAsync(
        int maQuanTri, int maDonHang, DoiTrangThaiDonHangRequest req, CancellationToken ct)
    {
        var trangThai = await db.DonHang.AsNoTracking()
            .Where(d => d.MaDonHang == maDonHang)
            .Select(d => d.TrangThai)
            .FirstOrDefaultAsync(ct);

        if (trangThai is null)
            return KetQua<DonHangChiTietDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy đơn hàng.");
        if (!TrangThaiDonHang.TiepTheo(trangThai).Contains(req.TrangThai))
            return KetQua<DonHangChiTietDto>.Loi(StatusCodes.Status400BadRequest,
                $"Không thể chuyển đơn hàng từ {trangThai} sang {req.TrangThai}.");

        // Thủ tục kiểm tra lại luồng (THROW 50002 → 400), hoàn kho khi DaHuy, đặt DaThanhToan khi HoanTat,
        // tạo ThongBao cho khách và ghi NhatKyHeThong.
        await db.DoiTrangThaiDonHangAsync(maDonHang, req.TrangThai, maQuanTri, ct);

        var ds = await TruyVanChiTietAsync(db.DonHang.Where(d => d.MaDonHang == maDonHang), ct);
        return KetQua<DonHangChiTietDto>.ThanhCong(ds[0]);
    }

    // =================================================================== Truy vấn chung

    private sealed class DongRaw
    {
        public int MaChiTietDH { get; init; }
        public int MaBienThe { get; init; }
        public int MaSanPham { get; init; }
        public string TenSanPham { get; init; } = null!;
        public string DuongDan { get; init; } = null!;
        public string? DanhSachAnh { get; init; }
        public string DungTich { get; init; } = null!;
        public string MaSKU { get; init; } = null!;
        public decimal DonGia { get; init; }
        public int SoLuong { get; init; }
        public decimal ThanhTien { get; init; }
        public bool DaDanhGia { get; init; }
    }

    private sealed class DonHangRaw
    {
        public int MaDonHang { get; init; }
        public DateTime NgayDat { get; init; }
        public DateTime? NgayCapNhat { get; init; }
        public string TrangThai { get; init; } = null!;
        public bool DaThanhToan { get; init; }
        public string TenNguoiNhan { get; init; } = null!;
        public string SoDienThoai { get; init; } = null!;
        public string DiaChiGiaoHang { get; init; } = null!;
        public string PhuongThucGiaoHang { get; init; } = null!;
        public string PhuongThucThanhToan { get; init; } = null!;
        public string? MaCode { get; init; }
        public decimal TienHang { get; init; }
        public decimal TienGiam { get; init; }
        public decimal PhiVanChuyen { get; init; }
        public decimal TongTien { get; init; }
        public int MaNguoiDung { get; init; }
        public string TenKhachHang { get; init; } = null!;
        public string EmailKhachHang { get; init; } = null!;
        public List<DongRaw> Dong { get; init; } = [];
    }

    /// <summary>Giữ nguyên thứ tự / phân trang của query đầu vào.</summary>
    private static async Task<List<DonHangChiTietDto>> TruyVanChiTietAsync(IQueryable<DonHangEntity> query, CancellationToken ct)
    {
        var raw = await query.AsNoTracking().AsSplitQuery()
            .Select(d => new DonHangRaw
            {
                MaDonHang = d.MaDonHang,
                NgayDat = d.NgayDat,
                NgayCapNhat = d.NgayCapNhat,
                TrangThai = d.TrangThai,
                DaThanhToan = d.DaThanhToan,
                TenNguoiNhan = d.TenNguoiNhan,
                SoDienThoai = d.SoDienThoai,
                DiaChiGiaoHang = d.DiaChiGiaoHang,
                PhuongThucGiaoHang = d.PhuongThucGiaoHang,
                PhuongThucThanhToan = d.PhuongThucThanhToan,
                MaCode = d.PhieuGiamGia != null ? d.PhieuGiamGia.MaCode : null,
                TienHang = d.TienHang,
                TienGiam = d.TienGiam,
                PhiVanChuyen = d.PhiVanChuyen,
                TongTien = d.TongTien,
                MaNguoiDung = d.MaNguoiDung,
                TenKhachHang = d.NguoiDung.HoTen,
                EmailKhachHang = d.NguoiDung.Email,
                Dong = d.ChiTietDonHangs
                    .OrderBy(c => c.MaChiTietDH)
                    .Select(c => new DongRaw
                    {
                        MaChiTietDH = c.MaChiTietDH,
                        MaBienThe = c.MaBienThe,
                        MaSanPham = c.BienThe.MaSanPham,
                        TenSanPham = c.BienThe.SanPham.Ten,
                        DuongDan = c.BienThe.SanPham.DuongDan,
                        DanhSachAnh = c.BienThe.SanPham.DanhSachAnh,
                        DungTich = c.BienThe.DungTich,
                        MaSKU = c.BienThe.MaSKU,
                        DonGia = c.DonGia,
                        SoLuong = c.SoLuong,
                        ThanhTien = c.ThanhTien,
                        DaDanhGia = c.DanhGia != null
                    })
                    .ToList()
            })
            .ToListAsync(ct);

        return raw.Select(d =>
        {
            var hoanTat = d.TrangThai == TrangThaiDonHang.HoanTat;
            var sanPham = d.Dong
                .GroupBy(c => new { c.MaBienThe, c.DonGia })
                .Select(g =>
                {
                    var c = g.First();
                    // Một sản phẩm đã mua chỉ cần một đánh giá: nhóm (có thể gồm nhiều lô) coi là
                    // đã đánh giá khi bất kỳ MaChiTietDH nào trong nhóm đã có DanhGia.
                    var daDanhGia = g.Any(x => x.DaDanhGia);
                    var coTheDanhGia = hoanTat && !daDanhGia;
                    return new DongDonHangDto(
                        c.MaBienThe, c.MaSanPham, c.TenSanPham, c.DuongDan,
                        JsonCot.DocMangChuoi(c.DanhSachAnh).FirstOrDefault(),
                        c.DungTich, c.MaSKU, c.DonGia,
                        g.Sum(x => x.SoLuong), g.Sum(x => x.ThanhTien),
                        g.Select(x => x.MaChiTietDH).ToList(),
                        daDanhGia,
                        coTheDanhGia,
                        coTheDanhGia ? c.MaChiTietDH : null);
                })
                .ToList();

            return new DonHangChiTietDto(
                d.MaDonHang, d.NgayDat, d.NgayCapNhat, d.TrangThai, d.DaThanhToan,
                d.TenNguoiNhan, d.SoDienThoai, d.DiaChiGiaoHang, d.PhuongThucGiaoHang, d.PhuongThucThanhToan,
                d.MaCode, d.TienHang, d.TienGiam, d.PhiVanChuyen, d.TongTien,
                d.MaNguoiDung, d.TenKhachHang, d.EmailKhachHang,
                sanPham,
                d.TrangThai == TrangThaiDonHang.ChoXacNhan,
                TrangThaiDonHang.TiepTheo(d.TrangThai));
        }).ToList();
    }
}
