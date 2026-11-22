using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.GioHang;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Services;

public interface IGioHangService
{
    Task<GioHangDto> LayAsync(int maNguoiDung, CancellationToken ct);
    Task<KetQua<GioHangDto>> ThemAsync(int maNguoiDung, ThemVaoGioRequest req, CancellationToken ct);
    Task<KetQua<GioHangDto>> CapNhatAsync(int maNguoiDung, int maBienThe, CapNhatGioRequest req, CancellationToken ct);
    Task<GioHangDto> XoaDongAsync(int maNguoiDung, int maBienThe, CancellationToken ct);
    Task<GioHangDto> XoaTatCaAsync(int maNguoiDung, CancellationToken ct);
}

public class GioHangService(AppDbContext db) : IGioHangService
{
    public async Task<GioHangDto> LayAsync(int maNguoiDung, CancellationToken ct)
    {
        var dong = await db.GioHang.AsNoTracking()
            .Where(g => g.MaNguoiDung == maNguoiDung)
            .OrderBy(g => g.NgayThem).ThenBy(g => g.MaBienThe)
            .Select(g => new
            {
                g.MaBienThe,
                g.SoLuong,
                g.BienThe.MaSanPham,
                TenSanPham = g.BienThe.SanPham.Ten,
                g.BienThe.SanPham.DuongDan,
                g.BienThe.SanPham.DanhSachAnh,
                TrangThaiSanPham = g.BienThe.SanPham.TrangThai,
                g.BienThe.DungTich,
                g.BienThe.MaSKU,
                g.BienThe.GiaBan,
                g.BienThe.GiaKhuyenMai,
                g.BienThe.KhuyenMaiTu,
                g.BienThe.KhuyenMaiDen,
                g.BienThe.SoLuongTon,
                g.BienThe.DangHoatDong
            })
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var danhSach = dong.Select(x =>
        {
            var donGia = GiaBan.ApDung(x.GiaBan, x.GiaKhuyenMai, x.KhuyenMaiTu, x.KhuyenMaiDen, now);
            var coTheMua = x.DangHoatDong && x.TrangThaiSanPham == TrangThaiSanPham.DangBan && x.SoLuongTon > 0;
            return new DongGioHangDto(
                x.MaBienThe, x.MaSanPham, x.TenSanPham, x.DuongDan,
                JsonCot.DocMangChuoi(x.DanhSachAnh).FirstOrDefault(),
                x.DungTich, x.MaSKU, x.GiaBan, donGia,
                GiaBan.KhuyenMaiHieuLuc(x.GiaKhuyenMai, x.KhuyenMaiTu, x.KhuyenMaiDen, now),
                x.SoLuong, x.SoLuongTon, donGia * x.SoLuong,
                coTheMua, x.SoLuong > x.SoLuongTon);
        }).ToList();

        return new GioHangDto(
            danhSach,
            danhSach.Sum(x => x.SoLuong),
            danhSach.Where(x => x.CoTheMua).Sum(x => x.ThanhTien),
            danhSach.Count > 0 && danhSach.All(x => x.CoTheMua && !x.VuotTonKho),
            BangPhiVanChuyen.TheoPhuongThuc);
    }

    public async Task<KetQua<GioHangDto>> ThemAsync(int maNguoiDung, ThemVaoGioRequest req, CancellationToken ct)
    {
        var bt = await db.BienThe.AsNoTracking()
            .Where(b => b.MaBienThe == req.MaBienThe)
            .Select(b => new { b.DangHoatDong, b.SoLuongTon, TrangThaiSanPham = b.SanPham.TrangThai })
            .FirstOrDefaultAsync(ct);

        if (bt is null)
            return KetQua<GioHangDto>.Loi(StatusCodes.Status404NotFound, "Biến thể sản phẩm không tồn tại.");
        if (!bt.DangHoatDong || bt.TrangThaiSanPham != TrangThaiSanPham.DangBan)
            return KetQua<GioHangDto>.Loi(StatusCodes.Status400BadRequest, "Sản phẩm này hiện không còn kinh doanh.");

        var dong = await db.GioHang.FindAsync([maNguoiDung, req.MaBienThe], ct);
        var daCo = dong?.SoLuong ?? 0;
        if (daCo + req.SoLuong > bt.SoLuongTon)
        {
            var thongBao = bt.SoLuongTon == 0
                ? "Sản phẩm đã hết hàng."
                : $"Chỉ còn {bt.SoLuongTon} sản phẩm" + (daCo > 0 ? $", giỏ hàng của bạn đã có {daCo}." : ".");
            return KetQua<GioHangDto>.Loi(StatusCodes.Status400BadRequest, thongBao);
        }

        if (dong is null)
            db.GioHang.Add(new GioHang { MaNguoiDung = maNguoiDung, MaBienThe = req.MaBienThe, SoLuong = req.SoLuong });
        else
            dong.SoLuong += req.SoLuong;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (ex.LaViPhamDuyNhat())
        {
            // Hai request thêm cùng biến thể đồng thời → PK_GioHang chặn bản thứ hai.
            return KetQua<GioHangDto>.Loi(StatusCodes.Status409Conflict, "Giỏ hàng vừa được cập nhật, vui lòng thử lại.");
        }

        return KetQua<GioHangDto>.ThanhCong(await LayAsync(maNguoiDung, ct));
    }

    public async Task<KetQua<GioHangDto>> CapNhatAsync(int maNguoiDung, int maBienThe, CapNhatGioRequest req, CancellationToken ct)
    {
        var dong = await db.GioHang.Include(g => g.BienThe)
            .FirstOrDefaultAsync(g => g.MaNguoiDung == maNguoiDung && g.MaBienThe == maBienThe, ct);
        if (dong is null)
            return KetQua<GioHangDto>.Loi(StatusCodes.Status404NotFound, "Sản phẩm không có trong giỏ hàng.");
        if (req.SoLuong > dong.BienThe.SoLuongTon)
            return KetQua<GioHangDto>.Loi(StatusCodes.Status400BadRequest,
                dong.BienThe.SoLuongTon == 0 ? "Sản phẩm đã hết hàng." : $"Chỉ còn {dong.BienThe.SoLuongTon} sản phẩm.");

        dong.SoLuong = req.SoLuong;
        await db.SaveChangesAsync(ct);
        return KetQua<GioHangDto>.ThanhCong(await LayAsync(maNguoiDung, ct));
    }

    public async Task<GioHangDto> XoaDongAsync(int maNguoiDung, int maBienThe, CancellationToken ct)
    {
        await db.GioHang.Where(g => g.MaNguoiDung == maNguoiDung && g.MaBienThe == maBienThe).ExecuteDeleteAsync(ct);
        return await LayAsync(maNguoiDung, ct);
    }

    public async Task<GioHangDto> XoaTatCaAsync(int maNguoiDung, CancellationToken ct)
    {
        await db.GioHang.Where(g => g.MaNguoiDung == maNguoiDung).ExecuteDeleteAsync(ct);
        return await LayAsync(maNguoiDung, ct);
    }
}
