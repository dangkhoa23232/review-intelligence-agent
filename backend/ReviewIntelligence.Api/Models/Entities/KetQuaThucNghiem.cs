namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.KetQuaThucNghiem – kết quả thực nghiệm từng mẫu theo mô hình.</summary>
public class KetQuaThucNghiem
{
    public long MaKetQua { get; set; }
    public int MaMau { get; set; }

    /// <summary>Xem <see cref="MoHinhAbsa"/>.</summary>
    public string MoHinh { get; set; } = null!;

    /// <summary>Xem <see cref="CamXuc"/>.</summary>
    public string CamXucDuDoan { get; set; } = null!;

    /// <summary>Chuỗi JSON (CK_KetQuaThucNghiem_KhiaCanhDuDoan).</summary>
    public string? KhiaCanhDuDoan { get; set; }

    public bool LaDung { get; set; }
    public string? NhomLoi { get; set; }
    public int? DoTreMs { get; set; }
    public DateTime NgayChay { get; set; }

    public MauKiemThu MauKiemThu { get; set; } = null!;
}
