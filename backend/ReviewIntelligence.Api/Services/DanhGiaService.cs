using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.DanhGia;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Services.Ai;
using DanhGiaEntity = ReviewIntelligence.Api.Models.Entities.DanhGia;

namespace ReviewIntelligence.Api.Services;

public interface IDanhGiaService
{
    Task<KetQua<DanhGiaDaTaoDto>> TaoAsync(int maNguoiDung, TaoDanhGiaRequest req, CancellationToken ct);
    Task<IReadOnlyList<DongDonCoTheDanhGiaDto>> DongDonCoTheDanhGiaAsync(int maNguoiDung, int maSanPham, CancellationToken ct);
    Task<KetQua<BaoCaoDanhGiaDto>> BaoCaoAsync(int maNguoiDung, int maDanhGia, BaoCaoDanhGiaRequest req, CancellationToken ct);
    Task<DanhSachDanhGiaQuanTriDto> DanhSachQuanTriAsync(DanhGiaQuanTriQuery q, CancellationToken ct);
    Task<KetQua<DoiTrangThaiDanhGiaResponse>> DoiTrangThaiAsync(int maQuanTri, int maDanhGia, DoiTrangThaiDanhGiaRequest req, CancellationToken ct);
}

public class DanhGiaService(AppDbContext db, IHangDoiPhanTichDanhGia hangDoiAi) : IDanhGiaService
{
    // ---------------------------------------------------------------- Khách hàng

    public async Task<KetQua<DanhGiaDaTaoDto>> TaoAsync(int maNguoiDung, TaoDanhGiaRequest req, CancellationToken ct)
    {
        if (!await db.SanPham.AnyAsync(x => x.MaSanPham == req.MaSanPham, ct))
            return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại.");

        // Các MaKhiaCanh phải tồn tại (FK_DiemTieuChi_MaKhiaCanh).
        var maKhiaCanh = req.DiemTieuChi.Select(x => x.MaKhiaCanh).Distinct().ToList();
        if (maKhiaCanh.Count > 0)
        {
            var tonTai = await db.KhiaCanh.Where(x => maKhiaCanh.Contains(x.MaKhiaCanh)).Select(x => x.MaKhiaCanh).ToListAsync(ct);
            var thieu = maKhiaCanh.Except(tonTai).ToList();
            if (thieu.Count > 0)
                return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status400BadRequest,
                    $"Khía cạnh không tồn tại: {string.Join(", ", thieu)}.");
        }

        // Xác thực đã mua hàng: dòng đơn thuộc người dùng, đúng sản phẩm, đơn HoanTat, chưa được đánh giá.
        var daMuaHang = false;
        if (req.MaChiTietDH is int maCt)
        {
            var dong = await db.ChiTietDonHang.AsNoTracking()
                .Where(x => x.MaChiTietDH == maCt)
                .Select(x => new
                {
                    x.DonHang.MaNguoiDung,
                    x.DonHang.TrangThai,
                    x.BienThe.MaSanPham,
                    // Cùng biến thể trong cùng đơn (các lô FEFO) chỉ được đánh giá một lần.
                    DaDanhGia = x.DonHang.ChiTietDonHangs.Any(o => o.MaBienThe == x.MaBienThe && o.DanhGia != null)
                })
                .FirstOrDefaultAsync(ct);

            if (dong is null || dong.MaNguoiDung != maNguoiDung)
                return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy dòng đơn hàng của bạn.");
            if (dong.MaSanPham != req.MaSanPham)
                return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status400BadRequest, "Dòng đơn hàng không chứa sản phẩm này.");
            if (dong.TrangThai != TrangThaiDonHang.HoanTat)
                return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status400BadRequest, "Chỉ được đánh giá khi đơn hàng đã hoàn tất.");
            if (dong.DaDanhGia)
                return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status409Conflict, "Sản phẩm này trong đơn hàng đã được đánh giá.");

            daMuaHang = true;
        }

        var danhGia = new DanhGiaEntity
        {
            MaSanPham = req.MaSanPham,
            MaNguoiDung = maNguoiDung,
            MaChiTietDH = req.MaChiTietDH,
            SoSao = req.SoSao,
            NoiDung = req.NoiDung.Trim(),
            DaMuaHang = daMuaHang,
            TrangThai = TrangThaiDanhGia.ChoDuyet,
            DiemTieuChis = req.DiemTieuChi
                .Select(x => new DiemTieuChi { MaKhiaCanh = x.MaKhiaCanh, Diem = x.Diem })
                .ToList()
        };
        db.DanhGia.Add(danhGia);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            // Hai request cùng lúc cho một MaChiTietDH → UX_DanhGia_MaChiTietDH chặn.
            return KetQua<DanhGiaDaTaoDto>.Loi(StatusCodes.Status409Conflict, "Dòng đơn hàng này đã được đánh giá.");
        }

        return KetQua<DanhGiaDaTaoDto>.ThanhCong(new DanhGiaDaTaoDto(
            danhGia.MaDanhGia, danhGia.MaSanPham, danhGia.TrangThai, danhGia.DaMuaHang, danhGia.NgayTao));
    }

    /// <summary>
    /// Mỗi (đơn HoanTat, biến thể) chưa đánh giá trả về một dòng – MaChiTietDH nhỏ nhất của nhóm,
    /// khớp MaChiTietDHDeDanhGia ở trang chi tiết đơn hàng.
    /// </summary>
    public async Task<IReadOnlyList<DongDonCoTheDanhGiaDto>> DongDonCoTheDanhGiaAsync(int maNguoiDung, int maSanPham, CancellationToken ct) =>
        await db.ChiTietDonHang.AsNoTracking()
            .Where(x => x.DonHang.MaNguoiDung == maNguoiDung
                        && x.DonHang.TrangThai == TrangThaiDonHang.HoanTat
                        && x.BienThe.MaSanPham == maSanPham
                        && !x.DonHang.ChiTietDonHangs.Any(o => o.MaBienThe == x.MaBienThe && o.DanhGia != null)
                        && !x.DonHang.ChiTietDonHangs.Any(o => o.MaBienThe == x.MaBienThe && o.MaChiTietDH < x.MaChiTietDH))
            .OrderByDescending(x => x.DonHang.NgayDat)
            .Select(x => new DongDonCoTheDanhGiaDto(x.MaChiTietDH, x.MaDonHang, x.BienThe.DungTich, x.DonHang.NgayDat))
            .ToListAsync(ct);

    public async Task<KetQua<BaoCaoDanhGiaDto>> BaoCaoAsync(int maNguoiDung, int maDanhGia, BaoCaoDanhGiaRequest req, CancellationToken ct)
    {
        var dg = await db.DanhGia.AsNoTracking()
            .Where(x => x.MaDanhGia == maDanhGia)
            .Select(x => new { x.TrangThai, x.MaNguoiDung })
            .FirstOrDefaultAsync(ct);

        // Chỉ báo cáo được đánh giá đang hiển thị công khai.
        if (dg is null || dg.TrangThai != TrangThaiDanhGia.DaDuyet)
            return KetQua<BaoCaoDanhGiaDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy đánh giá.");
        if (dg.MaNguoiDung == maNguoiDung)
            return KetQua<BaoCaoDanhGiaDto>.Loi(StatusCodes.Status400BadRequest, "Không thể báo cáo đánh giá của chính bạn.");
        if (await db.BaoCaoDanhGia.AnyAsync(x => x.MaDanhGia == maDanhGia && x.MaNguoiBaoCao == maNguoiDung, ct))
            return KetQua<BaoCaoDanhGiaDto>.Loi(StatusCodes.Status409Conflict, "Bạn đã báo cáo đánh giá này rồi.");

        var baoCao = new BaoCaoDanhGia
        {
            MaDanhGia = maDanhGia,
            MaNguoiBaoCao = maNguoiDung,
            LyDo = req.LyDo.Trim(),
            TrangThai = TrangThaiBaoCao.ChoXuLy
        };
        db.BaoCaoDanhGia.Add(baoCao);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            // UQ_BaoCaoDanhGia_MaDanhGia_MaNguoiBaoCao
            return KetQua<BaoCaoDanhGiaDto>.Loi(StatusCodes.Status409Conflict, "Bạn đã báo cáo đánh giá này rồi.");
        }

        return KetQua<BaoCaoDanhGiaDto>.ThanhCong(new BaoCaoDanhGiaDto(
            baoCao.MaBaoCao, baoCao.MaDanhGia, baoCao.LyDo, baoCao.TrangThai, baoCao.NgayTao));
    }

    // ---------------------------------------------------------------- Quản trị

    public async Task<DanhSachDanhGiaQuanTriDto> DanhSachQuanTriAsync(DanhGiaQuanTriQuery q, CancellationToken ct)
    {
        var soLuong = await db.DanhGia.AsNoTracking()
            .GroupBy(x => x.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.TrangThai, x => x.SoLuong, ct);
        foreach (var tt in TrangThaiDanhGia.TatCa) soLuong.TryAdd(tt, 0);

        var query = db.DanhGia.AsNoTracking();
        if (q.TrangThai is not null) query = query.Where(x => x.TrangThai == q.TrangThai);

        // Chờ duyệt: cũ nhất trước (xử lý theo thứ tự đến); còn lại: mới nhất trước.
        query = q.TrangThai == TrangThaiDanhGia.ChoDuyet
            ? query.OrderBy(x => x.NgayTao).ThenBy(x => x.MaDanhGia)
            : query.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaDanhGia);

        var tongSo = await query.CountAsync(ct);
        var duLieu = await query
            .Skip(q.BoQua).Take(q.KichThuoc)
            .Select(x => new DanhGiaQuanTriDto(
                x.MaDanhGia, x.MaSanPham, x.SanPham.Ten, x.SanPham.DuongDan,
                x.MaNguoiDung, x.NguoiDung.HoTen, x.NguoiDung.Email,
                x.SoSao, x.NoiDung, x.DaMuaHang, x.TrangThai, x.CamXuc, x.NgayTao, x.NgayCapNhat,
                x.BaoCaoDanhGias.Count(b => b.TrangThai == TrangThaiBaoCao.ChoXuLy),
                x.DiemTieuChis
                    .OrderBy(c => c.MaKhiaCanh)
                    .Select(c => new DiemTieuChiDto(c.MaKhiaCanh, c.KhiaCanh.Ten, c.Diem))
                    .ToList()))
            .ToListAsync(ct);

        return new DanhSachDanhGiaQuanTriDto(
            new KetQuaPhanTrang<DanhGiaQuanTriDto>(duLieu, tongSo, q.Trang, q.KichThuoc),
            soLuong);
    }

    public async Task<KetQua<DoiTrangThaiDanhGiaResponse>> DoiTrangThaiAsync(
        int maQuanTri, int maDanhGia, DoiTrangThaiDanhGiaRequest req, CancellationToken ct)
    {
        var dg = await db.DanhGia.FirstOrDefaultAsync(x => x.MaDanhGia == maDanhGia, ct);
        if (dg is null)
            return KetQua<DoiTrangThaiDanhGiaResponse>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy đánh giá.");

        var trangThaiCu = dg.TrangThai;
        if (trangThaiCu != req.TrangThai)
        {
            dg.TrangThai = req.TrangThai;
            dg.NgayCapNhat = DateTime.UtcNow;

            db.NhatKyHeThong.Add(new NhatKyHeThong
            {
                MaNguoiDung = maQuanTri,
                HanhDong = req.TrangThai == TrangThaiDanhGia.DaDuyet ? "DUYET_DANH_GIA" : "AN_DANH_GIA",
                DoiTuong = $"DanhGia#{maDanhGia}",
                ChiTiet = JsonSerializer.Serialize(new { TrangThaiCu = trangThaiCu, TrangThai = req.TrangThai })
            });

            // UPDATE DanhGia kích hoạt trigger trg_DanhGia_CapNhatDiem → SanPham.DiemTrungBinh/SoDanhGia
            // được tính lại ngay trong DB (EF đã khai báo HasTrigger nên không dùng OUTPUT).
            await db.SaveChangesAsync(ct);
        }

        var thongKe = await DocThongKeSanPhamSauTriggerAsync(dg.MaSanPham, ct);

        // Vừa chuyển sang DaDuyet → gửi ai-service phân tích ABSA + lưu vector Qdrant (bất đồng bộ, không chặn request).
        var daGuiAi = trangThaiCu != TrangThaiDanhGia.DaDuyet
                      && req.TrangThai == TrangThaiDanhGia.DaDuyet
                      && hangDoiAi.DuaVao(maDanhGia);

        // Rời DaDuyet (bị ẩn) → gỡ vector khỏi Qdrant và tính lại TomTatKhiaCanh.
        var daGuiGoBo = trangThaiCu == TrangThaiDanhGia.DaDuyet
                        && req.TrangThai != TrangThaiDanhGia.DaDuyet
                        && hangDoiAi.DuaVaoGoBo(maDanhGia);

        return KetQua<DoiTrangThaiDanhGiaResponse>.ThanhCong(
            new DoiTrangThaiDanhGiaResponse(maDanhGia, trangThaiCu, dg.TrangThai, thongKe, daGuiAi, daGuiGoBo));
    }

    /// <summary>
    /// Đọc DiemTrungBinh/SoDanhGia sau khi trigger chạy. Nếu SanPham đang được context track
    /// (giá trị cũ trong bộ nhớ) thì Reload() để đồng bộ với DB.
    /// </summary>
    private async Task<ThongKeSanPhamDto> DocThongKeSanPhamSauTriggerAsync(int maSanPham, CancellationToken ct)
    {
        var sanPham = db.ChangeTracker.Entries<SanPham>().FirstOrDefault(e => e.Entity.MaSanPham == maSanPham);
        if (sanPham is not null)
        {
            await sanPham.ReloadAsync(ct);
            return new ThongKeSanPhamDto(maSanPham, sanPham.Entity.DiemTrungBinh, sanPham.Entity.SoDanhGia);
        }

        // AsNoTracking luôn đọc thẳng từ DB nên luôn thấy giá trị trigger vừa cập nhật.
        return await db.SanPham.AsNoTracking()
            .Where(x => x.MaSanPham == maSanPham)
            .Select(x => new ThongKeSanPhamDto(x.MaSanPham, x.DiemTrungBinh, x.SoDanhGia))
            .FirstAsync(ct);
    }
}
