namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.BaoCaoDanhGia – báo cáo đánh giá không phù hợp.</summary>
public class BaoCaoDanhGia
{
    public int MaBaoCao { get; set; }
    public int MaDanhGia { get; set; }
    public int MaNguoiBaoCao { get; set; }
    public string LyDo { get; set; } = null!;

    /// <summary>Xem <see cref="TrangThaiBaoCao"/>.</summary>
    public string TrangThai { get; set; } = TrangThaiBaoCao.ChoXuLy;

    public DateTime NgayTao { get; set; }

    public DanhGia DanhGia { get; set; } = null!;
    public NguoiDung NguoiBaoCao { get; set; } = null!;
}
