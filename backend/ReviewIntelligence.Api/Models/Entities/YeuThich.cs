namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.YeuThich – danh sách yêu thích (PK kép).</summary>
public class YeuThich
{
    public int MaNguoiDung { get; set; }
    public int MaSanPham { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public SanPham SanPham { get; set; } = null!;
}
