namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.LoaiDa – loại da.</summary>
public class LoaiDa
{
    public int MaLoaiDa { get; set; }
    public string Ten { get; set; } = null!;

    public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
}
