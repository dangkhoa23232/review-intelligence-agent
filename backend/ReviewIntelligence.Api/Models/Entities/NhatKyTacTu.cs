namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.NhatKyTacTu – nhật ký các bước của tác tử (trace).</summary>
public class NhatKyTacTu
{
    public long MaBuoc { get; set; }
    public long MaTinNhan { get; set; }
    public int BuocSo { get; set; }

    /// <summary>Xem <see cref="LoaiBuocTacTu"/>.</summary>
    public string LoaiBuoc { get; set; } = null!;

    public string? TenCongCu { get; set; }

    /// <summary>Chuỗi JSON (CK_NhatKyTacTu_DauVao).</summary>
    public string? DauVao { get; set; }

    /// <summary>Chuỗi JSON (CK_NhatKyTacTu_DauRa).</summary>
    public string? DauRa { get; set; }

    public int? DoTreMs { get; set; }

    public TinNhan TinNhan { get; set; } = null!;
}
