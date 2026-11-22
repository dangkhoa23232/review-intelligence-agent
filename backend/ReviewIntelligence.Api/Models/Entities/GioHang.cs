namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.GioHang – giỏ hàng (PK kép MaNguoiDung + MaBienThe).</summary>
public class GioHang
{
    public int MaNguoiDung { get; set; }
    public int MaBienThe { get; set; }
    public int SoLuong { get; set; }
    public DateTime NgayThem { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public BienThe BienThe { get; set; } = null!;
}
