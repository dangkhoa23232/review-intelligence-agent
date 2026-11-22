using System.Security.Claims;

namespace ReviewIntelligence.Api.Infrastructure;

public static class ClaimsPrincipalExtensions
{
    /// <summary>MaNguoiDung lấy từ JWT (claim NameIdentifier). Chỉ gọi trong action có [Authorize].</summary>
    public static int LayMaNguoiDung(this ClaimsPrincipal user) =>
        int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var ma)
            ? ma
            : throw new InvalidOperationException("Token không chứa MaNguoiDung hợp lệ.");
}
