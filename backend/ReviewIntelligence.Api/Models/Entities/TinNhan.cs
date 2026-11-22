namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.TinNhan – tin nhắn hỏi đáp với tác tử (RAG).</summary>
public class TinNhan
{
    public long MaTinNhan { get; set; }
    public Guid MaPhien { get; set; }
    public int? MaNguoiDung { get; set; }
    public int? MaSanPham { get; set; }

    /// <summary>Xem <see cref="Entities.NguoiGui"/>.</summary>
    public string NguoiGui { get; set; } = null!;

    public string NoiDung { get; set; } = null!;

    /// <summary>Chuỗi JSON dẫn chứng (CK_TinNhan_DanChung).</summary>
    public string? DanChung { get; set; }

    public int? DoTreMs { get; set; }
    public DateTime NgayTao { get; set; }

    public NguoiDung? NguoiDung { get; set; }
    public SanPham? SanPham { get; set; }
    public ICollection<NhatKyTacTu> NhatKyTacTus { get; set; } = new List<NhatKyTacTu>();
}
