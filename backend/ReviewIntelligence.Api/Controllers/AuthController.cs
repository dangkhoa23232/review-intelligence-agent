using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.Auth;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

public class AuthController(IAuthService authService) : ApiControllerBase
{
    /// <summary>Đăng ký tài khoản khách hàng.</summary>
    [HttpPost("dang-ky")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(NguoiDungDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DangKy(DangKyRequest req, CancellationToken ct) =>
        TraVe(await authService.DangKyAsync(req, ct), StatusCodes.Status201Created);

    /// <summary>Đăng nhập, trả về JWT access token.</summary>
    [HttpPost("dang-nhap")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DangNhap(DangNhapRequest req, CancellationToken ct) =>
        TraVe(await authService.DangNhapAsync(req, ct));

    /// <summary>Thông tin người dùng hiện tại (đọc từ JWT).</summary>
    [HttpGet("toi")]
    [Authorize]
    public IActionResult Toi() => Ok(new
    {
        MaNguoiDung = User.FindFirstValue(ClaimTypes.NameIdentifier),
        Email = User.FindFirstValue(ClaimTypes.Email),
        HoTen = User.FindFirstValue(ClaimTypes.Name),
        VaiTro = User.FindFirstValue(ClaimTypes.Role)
    });
}
