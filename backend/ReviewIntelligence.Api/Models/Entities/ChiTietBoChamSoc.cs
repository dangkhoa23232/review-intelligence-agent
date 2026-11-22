namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.ChiTietBoChamSoc – các bước trong bộ chăm sóc (PK kép).</summary>
public class ChiTietBoChamSoc
{
    public int MaBo { get; set; }
    public int MaBienThe { get; set; }
    public byte ThuTuBuoc { get; set; }
    public string TenBuoc { get; set; } = null!;

    public BoChamSoc BoChamSoc { get; set; } = null!;
    public BienThe BienThe { get; set; } = null!;
}
