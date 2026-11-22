namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.BoChamSoc – bộ chăm sóc da (routine bundle).</summary>
public class BoChamSoc
{
    public int MaBo { get; set; }
    public string Ten { get; set; } = null!;
    public string? MoTa { get; set; }
    public int? MaLoaiDa { get; set; }
    public decimal PhanTramGiam { get; set; }

    public LoaiDa? LoaiDa { get; set; }
    public ICollection<ChiTietBoChamSoc> ChiTietBoChamSocs { get; set; } = new List<ChiTietBoChamSoc>();
}
