namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.DanhMuc – danh mục sản phẩm (cây cha–con).</summary>
public class DanhMuc
{
    public int MaDanhMuc { get; set; }
    public int? MaDanhMucCha { get; set; }
    public string Ten { get; set; } = null!;
    public string DuongDan { get; set; } = null!;

    public DanhMuc? DanhMucCha { get; set; }
    public ICollection<DanhMuc> DanhMucCons { get; set; } = new List<DanhMuc>();
    public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    public ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();
}
