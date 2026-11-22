namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>
/// Bảng dbo.KhuyenMai – khuyến mãi theo sản phẩm HOẶC danh mục
/// (CK_KhuyenMai_2: đúng một trong MaSanPham / MaDanhMuc khác NULL).
/// </summary>
public class KhuyenMai
{
    public int MaKhuyenMai { get; set; }
    public string Ten { get; set; } = null!;
    public decimal PhanTramGiam { get; set; }
    public int? MaSanPham { get; set; }
    public int? MaDanhMuc { get; set; }
    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }

    public SanPham? SanPham { get; set; }
    public DanhMuc? DanhMuc { get; set; }
}
