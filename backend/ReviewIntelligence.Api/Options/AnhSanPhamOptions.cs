using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.Options;

/// <summary>Nơi lưu ảnh sản phẩm tải lên (POST /api/admin/upload-anh).</summary>
public class AnhSanPhamOptions
{
    public const string SectionName = "AnhSanPham";

    /// <summary>
    /// Thư mục vật lý lưu ảnh – tương đối so với ContentRoot của API (thư mục dự án khi `dotnet run`).
    /// Mặc định là thư mục public của Vite để frontend phục vụ trực tiếp.
    /// </summary>
    [Required]
    public string ThuMucLuu { get; set; } = "../../frontend/public/images/san-pham";

    /// <summary>Tiền tố đường dẫn công khai ghi vào SanPham.DanhSachAnh.</summary>
    [Required, RegularExpression("^/.*", ErrorMessage = "DuongDanCongKhai phải bắt đầu bằng '/'.")]
    public string DuongDanCongKhai { get; set; } = "/images/san-pham";

    /// <summary>Dung lượng tối đa mỗi ảnh (MB).</summary>
    [Range(1, 20)]
    public int DungLuongToiDaMb { get; set; } = 5;
}
