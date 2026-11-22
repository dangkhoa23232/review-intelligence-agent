namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.KhiaCanh – khía cạnh / tiêu chí đánh giá (ABSA).</summary>
public class KhiaCanh
{
    public int MaKhiaCanh { get; set; }
    public string Ten { get; set; } = null!;
    public string? MoTa { get; set; }
}
