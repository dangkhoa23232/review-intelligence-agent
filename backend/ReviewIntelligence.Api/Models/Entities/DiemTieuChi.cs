namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.DiemTieuChi – điểm theo tiêu chí do người dùng chấm (PK kép).</summary>
public class DiemTieuChi
{
    public int MaDanhGia { get; set; }
    public int MaKhiaCanh { get; set; }

    /// <summary>1–5 (CK_DiemTieuChi_Diem).</summary>
    public byte Diem { get; set; }

    public DanhGia DanhGia { get; set; } = null!;
    public KhiaCanh KhiaCanh { get; set; } = null!;
}
