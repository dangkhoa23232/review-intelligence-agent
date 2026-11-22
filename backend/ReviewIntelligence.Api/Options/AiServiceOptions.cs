using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.Options;

/// <summary>Cấu hình gọi sang ai-service (FastAPI).</summary>
public class AiServiceOptions
{
    public const string SectionName = "AiService";

    /// <summary>Tắt để không gửi yêu cầu phân tích (ví dụ khi chưa chạy ai-service).</summary>
    public bool BatPhanTich { get; set; } = true;

    [Required, Url]
    public string BaseUrl { get; set; } = "http://localhost:8000/";

    [Range(1, 300)]
    public int ThoiGianChoGiay { get; set; } = 10;
}
