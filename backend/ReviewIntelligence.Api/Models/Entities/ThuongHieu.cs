namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.ThuongHieu – thương hiệu.</summary>
public class ThuongHieu
{
    public int MaThuongHieu { get; set; }
    public string Ten { get; set; } = null!;
    public string? QuocGia { get; set; }

    public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
}
