namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.KetQuaKhiaCanh – kết quả cảm xúc theo khía cạnh (ABSA) của một đánh giá.</summary>
public class KetQuaKhiaCanh
{
    public long MaKetQua { get; set; }
    public int MaDanhGia { get; set; }
    public int MaKhiaCanh { get; set; }

    /// <summary>Xem <see cref="Entities.CamXuc"/>.</summary>
    public string CamXuc { get; set; } = null!;

    public string? CumTuDanChung { get; set; }

    /// <summary>Xem <see cref="MoHinhAbsa"/>.</summary>
    public string MoHinh { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DanhGia DanhGia { get; set; } = null!;
    public KhiaCanh KhiaCanh { get; set; } = null!;
}
