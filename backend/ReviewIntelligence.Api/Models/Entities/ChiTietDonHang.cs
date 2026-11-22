namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.ChiTietDonHang – chi tiết đơn hàng.</summary>
public class ChiTietDonHang
{
    public int MaChiTietDH { get; set; }
    public int MaDonHang { get; set; }
    public int MaBienThe { get; set; }
    public int? MaLo { get; set; }
    public decimal DonGia { get; set; }
    public int SoLuong { get; set; }
    public decimal ThanhTien { get; set; }

    public DonHang DonHang { get; set; } = null!;
    public BienThe BienThe { get; set; } = null!;
    public LoHang? LoHang { get; set; }

    /// <summary>UX_DanhGia_MaChiTietDH: mỗi dòng đơn hàng được đánh giá tối đa 1 lần.</summary>
    public DanhGia? DanhGia { get; set; }
}
