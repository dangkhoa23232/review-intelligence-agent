namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.SanPham_LoaiDa – loại da phù hợp với sản phẩm (PK kép).</summary>
public class SanPhamLoaiDa
{
    public int MaSanPham { get; set; }
    public int MaLoaiDa { get; set; }

    public SanPham SanPham { get; set; } = null!;
    public LoaiDa LoaiDa { get; set; } = null!;
}
