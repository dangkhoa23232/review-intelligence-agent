namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.KhiaCanh – khía cạnh / tiêu chí đánh giá (ABSA).</summary>
public class KhiaCanh
{
    public int MaKhiaCanh { get; set; }
    public string Ten { get; set; } = null!;
    public string? MoTa { get; set; }

    public ICollection<DiemTieuChi> DiemTieuChis { get; set; } = new List<DiemTieuChi>();
    public ICollection<KetQuaKhiaCanh> KetQuaKhiaCanhs { get; set; } = new List<KetQuaKhiaCanh>();
    public ICollection<TomTatKhiaCanh> TomTatKhiaCanhs { get; set; } = new List<TomTatKhiaCanh>();
}
