namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.NhatKyHeThong – nhật ký thao tác (audit log).</summary>
public class NhatKyHeThong
{
    public long MaNhatKy { get; set; }
    public int? MaNguoiDung { get; set; }
    public string HanhDong { get; set; } = null!;
    public string DoiTuong { get; set; } = null!;

    /// <summary>Chuỗi JSON (CK_NhatKyHeThong_ChiTiet).</summary>
    public string? ChiTiet { get; set; }

    public DateTime NgayTao { get; set; }

    public NguoiDung? NguoiDung { get; set; }
}
