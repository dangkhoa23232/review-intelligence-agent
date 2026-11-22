using System.ComponentModel.DataAnnotations;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Validation;

namespace ReviewIntelligence.Api.DTOs.DonHang;

// ===================== Phiếu giảm giá =====================

/// <summary>Body của POST /api/phieu-giam-gia/kiem-tra.</summary>
public class KiemTraPhieuGiamGiaRequest
{
    /// <summary>dbo.PhieuGiamGia.MaCode VARCHAR(30).</summary>
    [Required(ErrorMessage = "Mã giảm giá là bắt buộc.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "Mã giảm giá tối đa 30 ký tự.")]
    public string MaCode { get; init; } = string.Empty;

    [Range(0, 10_000_000_000, ErrorMessage = "Tiền hàng không hợp lệ.")]
    public decimal TienHang { get; init; }
}

public record KiemTraPhieuGiamGiaResponse(
    string MaCode,
    string KieuGiam,
    decimal GiaTriGiam,
    decimal? GiamToiDa,
    decimal DonToiThieu,
    DateTime KetThuc,
    decimal TienHang,
    decimal TienGiam);

// ===================== Đặt hàng =====================

/// <summary>Body của POST /api/don-hang. Sản phẩm lấy từ giỏ hàng hiện tại của người dùng.</summary>
public class DatHangRequest
{
    [Required(ErrorMessage = "Tên người nhận là bắt buộc.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "Tên người nhận phải từ 2 đến 150 ký tự.")]
    public string TenNguoiNhan { get; init; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [MaxLength(20)]
    [RegularExpression(@"^\+?\d{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string SoDienThoai { get; init; } = string.Empty;

    [Required(ErrorMessage = "Địa chỉ giao hàng là bắt buộc.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Địa chỉ giao hàng phải từ 5 đến 300 ký tự.")]
    public string DiaChiGiaoHang { get; init; } = string.Empty;

    /// <summary>CK_DonHang_PhuongThucGiaoHang.</summary>
    [Required(ErrorMessage = "Phương thức giao hàng là bắt buộc.")]
    [ThuocTapGiaTri(Models.Entities.PhuongThucGiaoHang.TieuChuan, Models.Entities.PhuongThucGiaoHang.Nhanh)]
    public string PhuongThucGiaoHang { get; init; } = Models.Entities.PhuongThucGiaoHang.TieuChuan;

    /// <summary>CK_DonHang_PhuongThucThanhToan.</summary>
    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc.")]
    [ThuocTapGiaTri(Models.Entities.PhuongThucThanhToan.TienMat, Models.Entities.PhuongThucThanhToan.ChuyenKhoanGiaLap)]
    public string PhuongThucThanhToan { get; init; } = Models.Entities.PhuongThucThanhToan.TienMat;

    [MaxLength(30, ErrorMessage = "Mã giảm giá tối đa 30 ký tự.")]
    public string? MaCode { get; init; }
}

// ===================== Quản trị =====================

public class DonHangQuanTriQuery : PhanTrangQuery
{
    [ThuocTapGiaTri(TrangThaiDonHang.ChoXacNhan, TrangThaiDonHang.DaXacNhan, TrangThaiDonHang.DangDongGoi,
        TrangThaiDonHang.DangGiao, TrangThaiDonHang.HoanTat, TrangThaiDonHang.DaHuy)]
    public string? TrangThai { get; init; }
}

/// <summary>Body của PUT /api/admin/don-hang/{maDonHang}/trang-thai.</summary>
public class DoiTrangThaiDonHangRequest
{
    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    [ThuocTapGiaTri(TrangThaiDonHang.DaXacNhan, TrangThaiDonHang.DangDongGoi, TrangThaiDonHang.DangGiao,
        TrangThaiDonHang.HoanTat, TrangThaiDonHang.DaHuy)]
    public string TrangThai { get; init; } = string.Empty;
}

// ===================== Response =====================

/// <summary>Dòng sản phẩm của đơn (gộp các ChiTietDonHang cùng biến thể, cùng đơn giá – tách theo lô khi xuất kho FEFO).</summary>
public record DongDonHangDto(
    int MaBienThe,
    int MaSanPham,
    string TenSanPham,
    string DuongDan,
    string? AnhDaiDien,
    string DungTich,
    string MaSKU,
    decimal DonGia,
    int SoLuong,
    decimal ThanhTien,
    // Đơn HoanTat và còn dòng chưa được đánh giá.
    bool CoTheDanhGia);

public record DonHangChiTietDto(
    int MaDonHang,
    DateTime NgayDat,
    DateTime? NgayCapNhat,
    string TrangThai,
    bool DaThanhToan,
    string TenNguoiNhan,
    string SoDienThoai,
    string DiaChiGiaoHang,
    string PhuongThucGiaoHang,
    string PhuongThucThanhToan,
    string? MaCode,
    decimal TienHang,
    decimal TienGiam,
    decimal PhiVanChuyen,
    decimal TongTien,
    int MaNguoiDung,
    string TenKhachHang,
    string EmailKhachHang,
    IReadOnlyList<DongDonHangDto> SanPham,
    bool CoTheHuy,
    IReadOnlyList<string> TrangThaiTiepTheo);

public record DonHangTomTatDto(
    int MaDonHang,
    DateTime NgayDat,
    string TrangThai,
    bool DaThanhToan,
    decimal TongTien,
    int TongSoLuong,
    int SoMatHang,
    string? TenSanPhamDau,
    string? AnhDaiDien);

public record DanhSachDonHangQuanTriDto(
    KetQuaPhanTrang<DonHangChiTietDto> KetQua,
    IReadOnlyDictionary<string, int> SoLuongTheoTrangThai);
