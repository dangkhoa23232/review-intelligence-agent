using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.DTOs.GioHang;

/// <summary>Body của POST /api/gio-hang (cộng dồn nếu biến thể đã có trong giỏ).</summary>
public class ThemVaoGioRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã biến thể không hợp lệ.")]
    public int MaBienThe { get; init; }

    /// <summary>CK_GioHang_SoLuong: SoLuong &gt; 0.</summary>
    [Range(1, 999, ErrorMessage = "Số lượng phải từ 1 đến 999.")]
    public int SoLuong { get; init; } = 1;
}

/// <summary>Body của PUT /api/gio-hang/{maBienThe}.</summary>
public class CapNhatGioRequest
{
    [Range(1, 999, ErrorMessage = "Số lượng phải từ 1 đến 999.")]
    public int SoLuong { get; init; }
}

public record DongGioHangDto(
    int MaBienThe,
    int MaSanPham,
    string TenSanPham,
    string DuongDan,
    string? AnhDaiDien,
    string DungTich,
    string MaSKU,
    decimal GiaBan,
    // Đơn giá hiện hành (đã tính GiaKhuyenMai nếu còn hiệu lực).
    decimal DonGia,
    bool DangKhuyenMai,
    int SoLuong,
    int SoLuongTon,
    decimal ThanhTien,
    // false khi biến thể ngừng hoạt động / sản phẩm ngừng bán / hết hàng.
    bool CoTheMua,
    // true khi SoLuong trong giỏ lớn hơn SoLuongTon hiện tại.
    bool VuotTonKho);

public record GioHangDto(
    IReadOnlyList<DongGioHangDto> DanhSach,
    // Tổng số lượng mọi dòng – dùng cho badge giỏ hàng.
    int TongSoLuong,
    // Tổng thành tiền các dòng mua được.
    decimal TamTinh,
    // true khi giỏ không trống và mọi dòng đều mua được với số lượng hiện tại.
    bool CoTheDatHang,
    IReadOnlyDictionary<string, decimal> PhiVanChuyen);
