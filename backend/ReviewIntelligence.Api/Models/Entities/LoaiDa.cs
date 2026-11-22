namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.LoaiDa – loại da.</summary>
public class LoaiDa
{
    public int MaLoaiDa { get; set; }
    public string Ten { get; set; } = null!;

    public ICollection<NguoiDung> NguoiDungs { get; set; } = new List<NguoiDung>();
    public ICollection<SanPhamLoaiDa> SanPhamLoaiDas { get; set; } = new List<SanPhamLoaiDa>();
    public ICollection<BoChamSoc> BoChamSocs { get; set; } = new List<BoChamSoc>();
}
