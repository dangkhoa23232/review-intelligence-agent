using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Options;

namespace ReviewIntelligence.Api.Services;

public interface ITokenService
{
    (string Token, DateTime HetHanLuc) TaoAccessToken(NguoiDung nguoiDung);
}

public class TokenService(IOptions<JwtOptions> options) : ITokenService
{
    private readonly JwtOptions _jwt = options.Value;

    public (string Token, DateTime HetHanLuc) TaoAccessToken(NguoiDung nguoiDung)
    {
        var hetHan = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, nguoiDung.MaNguoiDung.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, nguoiDung.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, nguoiDung.MaNguoiDung.ToString()),
            new Claim(ClaimTypes.Name, nguoiDung.HoTen),
            new Claim(ClaimTypes.Role, nguoiDung.VaiTro)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: hetHan,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return (new JwtSecurityTokenHandler().WriteToken(token), hetHan);
    }
}
