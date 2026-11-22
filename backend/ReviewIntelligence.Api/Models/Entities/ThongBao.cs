namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.ThongBao – thông báo cho người dùng.</summary>
public class ThongBao
{
    public long MaThongBao { get; set; }
    public int MaNguoiDung { get; set; }
    public string NoiDung { get; set; } = null!;
    public string? LienKet { get; set; }
    public bool DaDoc { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung NguoiDung { get; set; } = null!;
}
