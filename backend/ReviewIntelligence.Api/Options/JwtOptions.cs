using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required] public string Issuer { get; set; } = string.Empty;
    [Required] public string Audience { get; set; } = string.Empty;

    /// <summary>Khóa ký HMAC-SHA256, tối thiểu 32 ký tự.</summary>
    [Required, MinLength(32)] public string SecretKey { get; set; } = string.Empty;

    [Range(1, 1440)] public int AccessTokenMinutes { get; set; } = 60;
}
