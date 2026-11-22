using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.DTOs.Auth;

public record DangNhapRequest(
    [property: Required(ErrorMessage = "Email là bắt buộc.")]
    [property: EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [property: MaxLength(255)]
    string Email,

    [property: Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [property: MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    string MatKhau);

public record DangKyRequest(
    [property: Required(ErrorMessage = "Email là bắt buộc.")]
    [property: EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [property: MaxLength(255)]
    string Email,

    [property: Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [property: MinLength(6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    [property: MaxLength(100)]
    string MatKhau,

    [property: Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [property: MaxLength(150)]
    string HoTen,

    [property: MaxLength(20)]
    [property: RegularExpression(@"^\+?\d{9,15}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    string? SoDienThoai,

    int? MaLoaiDa);

public record TokenResponse(string AccessToken, DateTime HetHanLuc, NguoiDungDto NguoiDung);

public record NguoiDungDto(int MaNguoiDung, string Email, string HoTen, string VaiTro, int? MaLoaiDa);
