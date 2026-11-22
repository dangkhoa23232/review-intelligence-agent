namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.PhieuGiamGia – phiếu giảm giá (voucher).</summary>
public class PhieuGiamGia
{
    public int MaPhieu { get; set; }
    public string MaCode { get; set; } = null!;

    /// <summary>Xem <see cref="Entities.KieuGiam"/>.</summary>
    public string KieuGiam { get; set; } = null!;

    public decimal GiaTriGiam { get; set; }
    public decimal? GiamToiDa { get; set; }
    public decimal DonToiThieu { get; set; }

    /// <summary>NULL = không giới hạn lượt dùng.</summary>
    public int? SoLuotConLai { get; set; }

    public DateTime BatDau { get; set; }
    public DateTime KetThuc { get; set; }

    public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
}
