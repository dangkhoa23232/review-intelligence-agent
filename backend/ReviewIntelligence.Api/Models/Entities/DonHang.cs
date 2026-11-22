namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>
/// Bảng dbo.DonHang – đơn hàng.
/// Đổi trạng thái phải qua thủ tục dbo.sp_DoiTrangThaiDonHang
/// (AppDbContext.DoiTrangThaiDonHangAsync), không cập nhật TrangThai trực tiếp.
/// </summary>
public class DonHang
{
    public int MaDonHang { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaPhieu { get; set; }
    public string TenNguoiNhan { get; set; } = null!;
    public string SoDienThoai { get; set; } = null!;
    public string DiaChiGiaoHang { get; set; } = null!;

    /// <summary>Xem <see cref="Entities.PhuongThucGiaoHang"/>.</summary>
    public string PhuongThucGiaoHang { get; set; } = null!;

    /// <summary>Xem <see cref="Entities.PhuongThucThanhToan"/>.</summary>
    public string PhuongThucThanhToan { get; set; } = null!;

    public decimal TienHang { get; set; }
    public decimal TienGiam { get; set; }
    public decimal PhiVanChuyen { get; set; }

    /// <summary>CK_DonHang_1: TongTien = TienHang - TienGiam + PhiVanChuyen.</summary>
    public decimal TongTien { get; set; }

    /// <summary>Xem <see cref="TrangThaiDonHang"/>.</summary>
    public string TrangThai { get; set; } = TrangThaiDonHang.ChoXacNhan;

    public bool DaThanhToan { get; set; }
    public DateTime NgayDat { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
    public PhieuGiamGia? PhieuGiamGia { get; set; }
    public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
}
