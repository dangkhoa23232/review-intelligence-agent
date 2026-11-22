namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.BienThe – biến thể sản phẩm (SKU).</summary>
public class BienThe
{
    public int MaBienThe { get; set; }
    public int MaSanPham { get; set; }
    public string MaSKU { get; set; } = null!;
    public string DungTich { get; set; } = null!;
    public decimal GiaBan { get; set; }
    public decimal? GiaKhuyenMai { get; set; }
    public DateTime? KhuyenMaiTu { get; set; }
    public DateTime? KhuyenMaiDen { get; set; }
    public int SoLuongTon { get; set; }
    public bool DangHoatDong { get; set; } = true;

    public SanPham SanPham { get; set; } = null!;
    public ICollection<LoHang> LoHangs { get; set; } = new List<LoHang>();
}
