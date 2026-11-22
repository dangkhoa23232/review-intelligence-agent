using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.DanhGia;
using ReviewIntelligence.Api.DTOs.SanPham;
using ReviewIntelligence.Api.DTOs.TraCuu;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Services;

public interface ISanPhamService
{
    Task<KetQuaPhanTrang<SanPhamTomTatDto>> TimKiemAsync(TimKiemSanPhamQuery q, CancellationToken ct);
    Task<KetQua<SanPhamChiTietDto>> LayChiTietAsync(string duongDan, CancellationToken ct);
    Task<KetQua<KetQuaPhanTrang<DanhGiaDto>>> LayDanhGiaAsync(string duongDan, PhanTrangQuery q, CancellationToken ct);
}

public class SanPhamService(AppDbContext db) : ISanPhamService
{
    /// <summary>Số đánh giá kèm theo trang chi tiết; xem thêm qua GET /api/san-pham/{duongDan}/danh-gia.</summary>
    public const int SoDanhGiaTrangChiTiet = 10;

    public async Task<KetQuaPhanTrang<SanPhamTomTatDto>> TimKiemAsync(TimKiemSanPhamQuery q, CancellationToken ct)
    {
        // Giá khuyến mãi của BienThe chỉ áp dụng khi now ∈ [KhuyenMaiTu, KhuyenMaiDen] (giờ UTC, giống SYSUTCDATETIME()).
        var now = DateTime.UtcNow;

        var sanPham = db.SanPham.AsNoTracking().Where(x => x.TrangThai == TrangThaiSanPham.DangBan);

        if (!string.IsNullOrWhiteSpace(q.TuKhoa))
        {
            var tk = q.TuKhoa.Trim();
            sanPham = sanPham.Where(x => x.Ten.Contains(tk) || (x.CongDung != null && x.CongDung.Contains(tk)));
        }
        if (q.MaDanhMuc is int maDm)
            sanPham = sanPham.Where(x => x.MaDanhMuc == maDm || x.DanhMuc.MaDanhMucCha == maDm);
        if (q.MaThuongHieu is int maTh)
            sanPham = sanPham.Where(x => x.MaThuongHieu == maTh);
        if (q.MaLoaiDa is int maLd)
            sanPham = sanPham.Where(x => x.SanPhamLoaiDas.Any(l => l.MaLoaiDa == maLd));

        var duLieu = sanPham.Select(x => new
        {
            x.MaSanPham,
            x.Ten,
            x.DuongDan,
            x.CongDung,
            x.DanhSachAnh,
            x.NgayTao,
            x.DiemTrungBinh,
            x.SoDanhGia,
            TenThuongHieu = x.ThuongHieu.Ten,
            TenDanhMuc = x.DanhMuc.Ten,
            GiaThapNhat = x.BienThes
                .Where(b => b.DangHoatDong)
                .Min(b => (decimal?)(b.GiaKhuyenMai != null && b.KhuyenMaiTu <= now && b.KhuyenMaiDen >= now
                    ? b.GiaKhuyenMai.Value
                    : b.GiaBan)),
            CoKhuyenMai = x.BienThes.Any(b => b.DangHoatDong && b.GiaKhuyenMai != null
                                              && b.KhuyenMaiTu <= now && b.KhuyenMaiDen >= now),
            ConHang = x.BienThes.Any(b => b.DangHoatDong && b.SoLuongTon > 0)
        });

        if (q.GiaTu is decimal giaTu) duLieu = duLieu.Where(x => x.GiaThapNhat >= giaTu);
        if (q.GiaDen is decimal giaDen) duLieu = duLieu.Where(x => x.GiaThapNhat <= giaDen);

        duLieu = q.SapXep switch
        {
            SapXepSanPham.GiaTang => duLieu.OrderBy(x => x.GiaThapNhat == null).ThenBy(x => x.GiaThapNhat).ThenBy(x => x.MaSanPham),
            SapXepSanPham.GiaGiam => duLieu.OrderBy(x => x.GiaThapNhat == null).ThenByDescending(x => x.GiaThapNhat).ThenBy(x => x.MaSanPham),
            SapXepSanPham.DanhGiaCao => duLieu.OrderByDescending(x => x.DiemTrungBinh).ThenByDescending(x => x.SoDanhGia).ThenBy(x => x.MaSanPham),
            _ => duLieu.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaSanPham)
        };

        var tongSo = await duLieu.CountAsync(ct);
        var trang = await duLieu.Skip(q.BoQua).Take(q.KichThuoc).ToListAsync(ct);

        var ketQua = trang.Select(x => new SanPhamTomTatDto(
            x.MaSanPham, x.Ten, x.DuongDan, x.CongDung,
            JsonCot.DocMangChuoi(x.DanhSachAnh).FirstOrDefault(),
            x.TenThuongHieu, x.TenDanhMuc,
            x.GiaThapNhat, x.CoKhuyenMai, x.ConHang,
            x.DiemTrungBinh, x.SoDanhGia)).ToList();

        return new KetQuaPhanTrang<SanPhamTomTatDto>(ketQua, tongSo, q.Trang, q.KichThuoc);
    }

    public async Task<KetQua<SanPhamChiTietDto>> LayChiTietAsync(string duongDan, CancellationToken ct)
    {
        var sp = await db.SanPham.AsNoTracking().AsSplitQuery()
            .Where(x => x.DuongDan == duongDan)
            .Select(x => new
            {
                x.MaSanPham, x.Ten, x.DuongDan, x.MoTa, x.CongDung, x.ThanhPhan, x.HuongDanSuDung,
                x.DanhSachAnh, x.TrangThai, x.DiemTrungBinh, x.SoDanhGia,
                DanhMuc = new DanhMucRutGonDto(x.DanhMuc.MaDanhMuc, x.DanhMuc.Ten, x.DanhMuc.DuongDan),
                ThuongHieu = new ThuongHieuDto(x.ThuongHieu.MaThuongHieu, x.ThuongHieu.Ten, x.ThuongHieu.QuocGia),
                LoaiDa = x.SanPhamLoaiDas
                    .OrderBy(l => l.MaLoaiDa)
                    .Select(l => new LoaiDaDto(l.LoaiDa.MaLoaiDa, l.LoaiDa.Ten))
                    .ToList(),
                BienThe = x.BienThes
                    .Where(b => b.DangHoatDong)
                    .OrderBy(b => b.GiaBan)
                    .Select(b => new { b.MaBienThe, b.MaSKU, b.DungTich, b.GiaBan, b.GiaKhuyenMai, b.KhuyenMaiTu, b.KhuyenMaiDen, b.SoLuongTon })
                    .ToList(),
                TomTat = x.TomTatKhiaCanhs
                    .OrderBy(t => t.MaKhiaCanh)
                    .Select(t => new
                    {
                        t.MaKhiaCanh, TenKhiaCanh = t.KhiaCanh.Ten, t.SoTichCuc, t.SoTieuCuc,
                        t.UuDiem, t.NhuocDiem, t.VanDeLapLai, t.DanChung, t.NgayCapNhat
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(ct);

        if (sp is null)
            return KetQua<SanPhamChiTietDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy sản phẩm.");

        var now = DateTime.UtcNow;
        var bienThe = sp.BienThe.Select(b =>
        {
            var conHieuLuc = b.GiaKhuyenMai is not null && b.KhuyenMaiTu <= now && b.KhuyenMaiDen >= now;
            var giaKm = conHieuLuc ? b.GiaKhuyenMai : null;
            return new BienTheDto(
                b.MaBienThe, b.MaSKU, b.DungTich, b.GiaBan,
                giaKm, conHieuLuc ? b.KhuyenMaiDen : null,
                giaKm ?? b.GiaBan,
                b.SoLuongTon, b.SoLuongTon > 0);
        }).ToList();

        var tomTat = sp.TomTat.Select(t => new TomTatKhiaCanhDto(
            t.MaKhiaCanh, t.TenKhiaCanh, t.SoTichCuc, t.SoTieuCuc,
            t.UuDiem, t.NhuocDiem, t.VanDeLapLai,
            JsonCot.DocMangSo(t.DanChung), t.NgayCapNhat)).ToList();

        var danhGia = await TruyVanDanhGiaDaDuyetAsync(sp.MaSanPham, 1, SoDanhGiaTrangChiTiet, ct);

        return KetQua<SanPhamChiTietDto>.ThanhCong(new SanPhamChiTietDto(
            sp.MaSanPham, sp.Ten, sp.DuongDan, sp.MoTa, sp.CongDung, sp.ThanhPhan, sp.HuongDanSuDung,
            JsonCot.DocMangChuoi(sp.DanhSachAnh), sp.TrangThai, sp.DiemTrungBinh, sp.SoDanhGia,
            sp.DanhMuc, sp.ThuongHieu, sp.LoaiDa, bienThe, tomTat, danhGia));
    }

    public async Task<KetQua<KetQuaPhanTrang<DanhGiaDto>>> LayDanhGiaAsync(string duongDan, PhanTrangQuery q, CancellationToken ct)
    {
        var maSanPham = await db.SanPham.AsNoTracking()
            .Where(x => x.DuongDan == duongDan)
            .Select(x => (int?)x.MaSanPham)
            .FirstOrDefaultAsync(ct);

        if (maSanPham is null)
            return KetQua<KetQuaPhanTrang<DanhGiaDto>>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy sản phẩm.");

        return KetQua<KetQuaPhanTrang<DanhGiaDto>>.ThanhCong(
            await TruyVanDanhGiaDaDuyetAsync(maSanPham.Value, q.Trang, q.KichThuoc, ct));
    }

    /// <summary>Chỉ đánh giá TrangThai = DaDuyet, mới nhất trước.</summary>
    private async Task<KetQuaPhanTrang<DanhGiaDto>> TruyVanDanhGiaDaDuyetAsync(int maSanPham, int trang, int kichThuoc, CancellationToken ct)
    {
        var query = db.DanhGia.AsNoTracking()
            .Where(d => d.MaSanPham == maSanPham && d.TrangThai == TrangThaiDanhGia.DaDuyet);

        var tongSo = await query.CountAsync(ct);
        var duLieu = await query
            .OrderByDescending(d => d.NgayTao).ThenByDescending(d => d.MaDanhGia)
            .Skip((trang - 1) * kichThuoc).Take(kichThuoc)
            .Select(d => new DanhGiaDto(
                d.MaDanhGia, d.SoSao, d.NoiDung, d.DaMuaHang, d.CamXuc, d.NgayTao,
                d.NguoiDung.HoTen,
                d.ChiTietDonHang != null ? d.ChiTietDonHang.BienThe.DungTich : null,
                d.DiemTieuChis
                    .OrderBy(c => c.MaKhiaCanh)
                    .Select(c => new DiemTieuChiDto(c.MaKhiaCanh, c.KhiaCanh.Ten, c.Diem))
                    .ToList()))
            .ToListAsync(ct);

        return new KetQuaPhanTrang<DanhGiaDto>(duLieu, tongSo, trang, kichThuoc);
    }
}
