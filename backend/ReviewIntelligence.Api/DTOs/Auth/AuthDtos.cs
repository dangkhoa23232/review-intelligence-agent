using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.DTOs.Auth;

// Request dùng class + init (không dùng positional record): ASP.NET Core không chấp nhận
// attribute validation gắn lên property của tham số constructor record.

public class DangNhapRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    public string MatKhau { get; init; } = string.Empty;
}

public class DangKyRequest
{
    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    [MaxLength(100)]
    public string MatKhau { get; init; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [MaxLength(150)]
    public string HoTen { get; init; } = string.Empty;

    [MaxLength(20)]
    [RegularExpression(@"^\+?\d{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string? SoDienThoai { get; init; }

    public int? MaLoaiDa { get; init; }
}

public record TokenResponse(string AccessToken, DateTime HetHanLuc, NguoiDungDto NguoiDung);

public record NguoiDungDto(int MaNguoiDung, string Email, string HoTen, string VaiTro, int? MaLoaiDa);
