namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.LoHang – lô hàng và hạn sử dụng.</summary>
public class LoHang
{
    public int MaLo { get; set; }
    public int MaBienThe { get; set; }
    public string SoLo { get; set; } = null!;
    public DateOnly HanSuDung { get; set; }
    public int SoLuongNhap { get; set; }
    public int SoLuongConLai { get; set; }
    public DateTime NgayNhap { get; set; }

    public BienThe BienThe { get; set; } = null!;
}
