using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.SanPham;
using ReviewIntelligence.Api.DTOs.TraCuu;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Services;

public interface IAdminSanPhamService
{
    Task<DanhSachSanPhamQuanTriDto> DanhSachAsync(SanPhamQuanTriQuery q, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> ChiTietAsync(int maSanPham, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> TaoAsync(LuuSanPhamRequest req, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> CapNhatAsync(int maSanPham, LuuSanPhamRequest req, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> DoiTrangThaiAsync(int maSanPham, string trangThai, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> ThemBienTheAsync(int maSanPham, LuuBienTheRequest req, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> CapNhatBienTheAsync(int maSanPham, int maBienThe, LuuBienTheRequest req, CancellationToken ct);
    Task<KetQua<SanPhamQuanTriChiTietDto>> NhapLoHangAsync(int maSanPham, int maBienThe, NhapLoHangRequest req, CancellationToken ct);
}

/// <summary>Quản trị dbo.SanPham, SanPham_LoaiDa, BienThe, LoHang.</summary>
public class AdminSanPhamService(AppDbContext db, ILogger<AdminSanPhamService> logger) : IAdminSanPhamService
{
    // =================================================================== Đọc

    public async Task<DanhSachSanPhamQuanTriDto> DanhSachAsync(SanPhamQuanTriQuery q, CancellationToken ct)
    {
        var loc = db.SanPham.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(q.TuKhoa))
        {
            var tk = q.TuKhoa.Trim();
            loc = loc.Where(x => x.Ten.Contains(tk) || x.DuongDan.Contains(tk)
                                 || x.BienThes.Any(b => b.MaSKU.Contains(tk)));
        }
        if (q.MaDanhMuc is int maDm) loc = loc.Where(x => x.MaDanhMuc == maDm || x.DanhMuc.MaDanhMucCha == maDm);
        if (q.MaThuongHieu is int maTh) loc = loc.Where(x => x.MaThuongHieu == maTh);

        // Đếm theo trạng thái trên tập đã lọc (trừ chính bộ lọc trạng thái) để hiện số trên tab.
        var demTheoTrangThai = await loc.GroupBy(x => x.TrangThai)
            .Select(g => new { g.Key, SoLuong = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.SoLuong, ct);
        var soLuong = TrangThaiSanPham.TatCa.ToDictionary(t => t, t => demTheoTrangThai.GetValueOrDefault(t));

        if (q.TrangThai is { } tt) loc = loc.Where(x => x.TrangThai == tt);

        var tongSo = await loc.CountAsync(ct);
        var trang = await loc
            .OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaSanPham)
            .Skip(q.BoQua).Take(q.KichThuoc)
            .Select(x => new
            {
                x.MaSanPham, x.Ten, x.DuongDan, x.DanhSachAnh, x.TrangThai, x.DiemTrungBinh, x.SoDanhGia, x.NgayTao,
                TenThuongHieu = x.ThuongHieu.Ten,
                TenDanhMuc = x.DanhMuc.Ten,
                SoBienThe = x.BienThes.Count(),
                TongTon = x.BienThes.Where(b => b.DangHoatDong).Sum(b => (int?)b.SoLuongTon) ?? 0,
                GiaThapNhat = x.BienThes.Where(b => b.DangHoatDong).Min(b => (decimal?)b.GiaBan)
            })
            .ToListAsync(ct);

        var duLieu = trang.Select(x => new SanPhamQuanTriDto(
            x.MaSanPham, x.Ten, x.DuongDan, JsonCot.DocMangChuoi(x.DanhSachAnh).FirstOrDefault(),
            x.TenThuongHieu, x.TenDanhMuc, x.TrangThai, x.SoBienThe, x.TongTon, x.GiaThapNhat,
            x.DiemTrungBinh, x.SoDanhGia, x.NgayTao)).ToList();

        return new DanhSachSanPhamQuanTriDto(
            new KetQuaPhanTrang<SanPhamQuanTriDto>(duLieu, tongSo, q.Trang, q.KichThuoc), soLuong);
    }

    public async Task<KetQua<SanPhamQuanTriChiTietDto>> ChiTietAsync(int maSanPham, CancellationToken ct)
    {
        var sp = await db.SanPham.AsNoTracking().AsSplitQuery()
            .Where(x => x.MaSanPham == maSanPham)
            .Select(x => new
            {
                x.MaSanPham, x.MaDanhMuc, x.MaThuongHieu, x.Ten, x.DuongDan, x.MoTa, x.CongDung, x.ThanhPhan,
                x.HuongDanSuDung, x.DanhSachAnh, x.TrangThai, x.DiemTrungBinh, x.SoDanhGia, x.NgayTao,
                LoaiDa = x.SanPhamLoaiDas.OrderBy(l => l.MaLoaiDa)
                    .Select(l => new LoaiDaDto(l.LoaiDa.MaLoaiDa, l.LoaiDa.Ten)).ToList(),
                BienThe = x.BienThes.OrderBy(b => b.MaBienThe)
                    .Select(b => new BienTheQuanTriDto(
                        b.MaBienThe, b.MaSKU, b.DungTich, b.GiaBan, b.GiaKhuyenMai, b.KhuyenMaiTu, b.KhuyenMaiDen,
                        b.SoLuongTon, b.DangHoatDong,
                        b.LoHangs.OrderBy(l => l.HanSuDung).ThenBy(l => l.MaLo)
                            .Select(l => new LoHangDto(l.MaLo, l.SoLo, l.HanSuDung, l.SoLuongNhap, l.SoLuongConLai, l.NgayNhap))
                            .ToList()))
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (sp is null) return KhongThay<SanPhamQuanTriChiTietDto>();

        return KetQua<SanPhamQuanTriChiTietDto>.ThanhCong(new SanPhamQuanTriChiTietDto(
            sp.MaSanPham, sp.MaDanhMuc, sp.MaThuongHieu, sp.Ten, sp.DuongDan, sp.MoTa, sp.CongDung, sp.ThanhPhan,
            sp.HuongDanSuDung, JsonCot.DocMangChuoi(sp.DanhSachAnh), sp.TrangThai, sp.DiemTrungBinh, sp.SoDanhGia,
            sp.NgayTao, sp.LoaiDa, sp.BienThe));
    }

    // =================================================================== Sản phẩm

    public async Task<KetQua<SanPhamQuanTriChiTietDto>> TaoAsync(LuuSanPhamRequest req, CancellationToken ct)
    {
        var loi = await KiemTraThamChieuAsync(req, ct);
        if (loi is not null) return KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status400BadRequest, loi);

        var duongDan = await XacDinhDuongDanAsync(req, null, ct);
        if (!duongDan.OK) return KetQua<SanPhamQuanTriChiTietDto>.Loi(duongDan.MaLoi, duongDan.ThongBao!);

        var sp = new SanPham { DuongDan = duongDan.DuLieu! };
        GanThongTin(sp, req);
        sp.SanPhamLoaiDas = req.MaLoaiDa.Distinct().Select(m => new SanPhamLoaiDa { MaLoaiDa = m }).ToList();
        db.SanPham.Add(sp);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            return KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status409Conflict, $"Đường dẫn '{sp.DuongDan}' đã được dùng cho sản phẩm khác.");
        }

        logger.LogInformation("Tạo SanPham#{MaSanPham} '{Ten}'.", sp.MaSanPham, sp.Ten);
        return await ChiTietAsync(sp.MaSanPham, ct);
    }

    public async Task<KetQua<SanPhamQuanTriChiTietDto>> CapNhatAsync(int maSanPham, LuuSanPhamRequest req, CancellationToken ct)
    {
        var sp = await db.SanPham.Include(x => x.SanPhamLoaiDas).FirstOrDefaultAsync(x => x.MaSanPham == maSanPham, ct);
        if (sp is null) return KhongThay<SanPhamQuanTriChiTietDto>();

        var loi = await KiemTraThamChieuAsync(req, ct);
        if (loi is not null) return KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status400BadRequest, loi);

        var duongDan = await XacDinhDuongDanAsync(req, sp, ct);
        if (!duongDan.OK) return KetQua<SanPhamQuanTriChiTietDto>.Loi(duongDan.MaLoi, duongDan.ThongBao!);

        sp.DuongDan = duongDan.DuLieu!;
        GanThongTin(sp, req);

        // Đồng bộ SanPham_LoaiDa: xóa loại không còn chọn, thêm loại mới.
        var moi = req.MaLoaiDa.Distinct().ToHashSet();
        foreach (var l in sp.SanPhamLoaiDas.Where(l => !moi.Contains(l.MaLoaiDa)).ToList())
            sp.SanPhamLoaiDas.Remove(l);
        foreach (var m in moi.Where(m => sp.SanPhamLoaiDas.All(l => l.MaLoaiDa != m)))
            sp.SanPhamLoaiDas.Add(new SanPhamLoaiDa { MaSanPham = sp.MaSanPham, MaLoaiDa = m });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            return KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status409Conflict, $"Đường dẫn '{sp.DuongDan}' đã được dùng cho sản phẩm khác.");
        }

        logger.LogInformation("Cập nhật SanPham#{MaSanPham}.", maSanPham);
        return await ChiTietAsync(maSanPham, ct);
    }

    public async Task<KetQua<SanPhamQuanTriChiTietDto>> DoiTrangThaiAsync(int maSanPham, string trangThai, CancellationToken ct)
    {
        var soDong = await db.SanPham.Where(x => x.MaSanPham == maSanPham)
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.TrangThai, trangThai), ct);
        if (soDong == 0) return KhongThay<SanPhamQuanTriChiTietDto>();

        logger.LogInformation("SanPham#{MaSanPham} → {TrangThai}.", maSanPham, trangThai);
        return await ChiTietAsync(maSanPham, ct);
    }

    // =================================================================== Biến thể

    public async Task<KetQua<SanPhamQuanTriChiTietDto>> ThemBienTheAsync(int maSanPham, LuuBienTheRequest req, CancellationToken ct)
    {
        if (!await db.SanPham.AnyAsync(x => x.MaSanPham == maSanPham, ct)) return KhongThay<SanPhamQuanTriChiTietDto>();

        var bt = new BienThe { MaSanPham = maSanPham, SoLuongTon = 0 };
        GanBienThe(bt, req);
        db.BienThe.Add(bt);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            return TrungSku(req.MaSKU.Trim().ToUpperInvariant());
        }

        logger.LogInformation("Thêm BienThe#{MaBienThe} ({MaSKU}) cho SanPham#{MaSanPham}.", bt.MaBienThe, bt.MaSKU, maSanPham);
        return await ChiTietAsync(maSanPham, ct);
    }

    public async Task<KetQua<SanPhamQuanTriChiTietDto>> CapNhatBienTheAsync(int maSanPham, int maBienThe, LuuBienTheRequest req, CancellationToken ct)
    {
        var bt = await db.BienThe.FirstOrDefaultAsync(x => x.MaBienThe == maBienThe && x.MaSanPham == maSanPham, ct);
        if (bt is null)
            return KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy biến thể của sản phẩm này.");

        GanBienThe(bt, req);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            return TrungSku(req.MaSKU.Trim().ToUpperInvariant());
        }

        logger.LogInformation("Cập nhật BienThe#{MaBienThe}.", maBienThe);
        return await ChiTietAsync(maSanPham, ct);
    }

    // =================================================================== Lô hàng

    /// <summary>
    /// Thêm LoHang và cộng dồn BienThe.SoLuongTon trong cùng một transaction.
    /// Tăng tồn bằng UPDATE ... SET SoLuongTon = SoLuongTon + n (nguyên tử) nên không ghi đè
    /// lượng tồn vừa bị trừ bởi một đơn hàng đặt đồng thời.
    /// </summary>
    public async Task<KetQua<SanPhamQuanTriChiTietDto>> NhapLoHangAsync(int maSanPham, int maBienThe, NhapLoHangRequest req, CancellationToken ct)
    {
        var hanSuDung = req.HanSuDung!.Value;
        if (hanSuDung <= DateOnly.FromDateTime(DateTime.UtcNow))
            return KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status400BadRequest, "Hạn sử dụng phải sau ngày hôm nay.");

        var soLo = req.SoLo.Trim();
        var strategy = db.Database.CreateExecutionStrategy();
        var kq = await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var tx = await db.Database.BeginTransactionAsync(ct);

            var soDong = await db.BienThe
                .Where(x => x.MaBienThe == maBienThe && x.MaSanPham == maSanPham)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.SoLuongTon, x => x.SoLuongTon + req.SoLuongNhap), ct);
            if (soDong == 0)
                return KetQua<bool>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy biến thể của sản phẩm này.");

            db.LoHang.Add(new LoHang
            {
                MaBienThe = maBienThe,
                SoLo = soLo,
                HanSuDung = hanSuDung,
                SoLuongNhap = req.SoLuongNhap,
                SoLuongConLai = req.SoLuongNhap
            });

            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
            {
                await tx.RollbackAsync(ct);
                db.ChangeTracker.Clear();
                return KetQua<bool>.Loi(StatusCodes.Status409Conflict, $"Số lô '{soLo}' đã tồn tại cho biến thể này.");
            }

            await tx.CommitAsync(ct);
            return KetQua<bool>.ThanhCong(true);
        });

        if (!kq.OK) return KetQua<SanPhamQuanTriChiTietDto>.Loi(kq.MaLoi, kq.ThongBao!);

        logger.LogInformation("Nhập lô {SoLo} ({SoLuong}) cho BienThe#{MaBienThe}.", soLo, req.SoLuongNhap, maBienThe);
        return await ChiTietAsync(maSanPham, ct);
    }

    // =================================================================== Hỗ trợ

    private static KetQua<T> KhongThay<T>() =>
        KetQua<T>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy sản phẩm.");

    private static KetQua<SanPhamQuanTriChiTietDto> TrungSku(string sku) =>
        KetQua<SanPhamQuanTriChiTietDto>.Loi(StatusCodes.Status409Conflict, $"Mã SKU '{sku}' đã tồn tại.");

    private static void GanThongTin(SanPham sp, LuuSanPhamRequest req)
    {
        sp.MaDanhMuc = req.MaDanhMuc;
        sp.MaThuongHieu = req.MaThuongHieu;
        sp.Ten = req.Ten.Trim();
        sp.MoTa = RongThanhNull(req.MoTa);
        sp.CongDung = RongThanhNull(req.CongDung);
        sp.ThanhPhan = RongThanhNull(req.ThanhPhan);
        sp.HuongDanSuDung = RongThanhNull(req.HuongDanSuDung);
        var anh = req.DanhSachAnh.Select(a => a.Trim()).ToList();
        sp.DanhSachAnh = anh.Count == 0 ? null : JsonSerializer.Serialize(anh);
        sp.TrangThai = req.TrangThai;
    }

    private static void GanBienThe(BienThe bt, LuuBienTheRequest req)
    {
        bt.MaSKU = req.MaSKU.Trim().ToUpperInvariant();
        bt.DungTich = req.DungTich.Trim();
        bt.GiaBan = req.GiaBan;
        bt.GiaKhuyenMai = req.GiaKhuyenMai;
        bt.KhuyenMaiTu = req.GiaKhuyenMai is null ? null : req.KhuyenMaiTu;
        bt.KhuyenMaiDen = req.GiaKhuyenMai is null ? null : req.KhuyenMaiDen;
        bt.DangHoatDong = req.DangHoatDong;
    }

    private static string? RongThanhNull(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    /// <summary>Danh mục, thương hiệu, loại da phải tồn tại – trả thông báo lỗi hoặc null.</summary>
    private async Task<string?> KiemTraThamChieuAsync(LuuSanPhamRequest req, CancellationToken ct)
    {
        if (!await db.DanhMuc.AnyAsync(x => x.MaDanhMuc == req.MaDanhMuc, ct)) return "Danh mục không tồn tại.";
        if (!await db.ThuongHieu.AnyAsync(x => x.MaThuongHieu == req.MaThuongHieu, ct)) return "Thương hiệu không tồn tại.";

        var maLoaiDa = req.MaLoaiDa.Distinct().ToList();
        if (maLoaiDa.Count > 0 && await db.LoaiDa.CountAsync(x => maLoaiDa.Contains(x.MaLoaiDa), ct) != maLoaiDa.Count)
            return "Có loại da không tồn tại.";
        return null;
    }

    /// <summary>
    /// Đường dẫn nhập tay phải chưa ai dùng (409). Bỏ trống → giữ đường dẫn cũ khi sửa,
    /// hoặc sinh từ tên khi tạo (thêm hậu tố -2, -3… nếu trùng).
    /// </summary>
    private async Task<KetQua<string>> XacDinhDuongDanAsync(LuuSanPhamRequest req, SanPham? hienTai, CancellationToken ct)
    {
        var maHienTai = hienTai?.MaSanPham ?? 0;
        if (!string.IsNullOrWhiteSpace(req.DuongDan))
        {
            var dd = req.DuongDan.Trim();
            return await db.SanPham.AnyAsync(x => x.DuongDan == dd && x.MaSanPham != maHienTai, ct)
                ? KetQua<string>.Loi(StatusCodes.Status409Conflict, $"Đường dẫn '{dd}' đã được dùng cho sản phẩm khác.")
                : KetQua<string>.ThanhCong(dd);
        }
        if (hienTai is not null) return KetQua<string>.ThanhCong(hienTai.DuongDan);

        var goc = TaoDuongDan(req.Ten);
        if (goc.Length == 0) goc = "san-pham";
        var daDung = await db.SanPham.Where(x => x.DuongDan == goc || x.DuongDan.StartsWith(goc + "-"))
            .Select(x => x.DuongDan).ToListAsync(ct);
        var ung = goc;
        for (var i = 2; daDung.Contains(ung); i++) ung = $"{goc}-{i}";
        return KetQua<string>.ThanhCong(ung);
    }

    /// <summary>"Sữa rửa mặt Senka 120g" → "sua-rua-mat-senka-120g".</summary>
    public static string TaoDuongDan(string ten)
    {
        var chuan = ten.Trim().ToLowerInvariant().Replace('đ', 'd').Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(chuan.Length);
        var gachCuoi = true;
        foreach (var c in chuan)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
            if (c is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                sb.Append(c);
                gachCuoi = false;
            }
            else if (!gachCuoi)
            {
                sb.Append('-');
                gachCuoi = true;
            }
        }
        var kq = sb.ToString().Trim('-');
        return kq.Length > 200 ? kq[..200].TrimEnd('-') : kq;
    }
}
