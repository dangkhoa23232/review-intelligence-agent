namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.TomTatKhiaCanh – tóm tắt ưu/nhược điểm theo sản phẩm × khía cạnh.</summary>
public class TomTatKhiaCanh
{
    public int MaTomTat { get; set; }
    public int MaSanPham { get; set; }
    public int MaKhiaCanh { get; set; }
    public int SoTichCuc { get; set; }
    public int SoTieuCuc { get; set; }
    public string? UuDiem { get; set; }
    public string? NhuocDiem { get; set; }
    public string? VanDeLapLai { get; set; }

    /// <summary>Chuỗi JSON danh sách dẫn chứng (CK_TomTatKhiaCanh_DanChung).</summary>
    public string? DanChung { get; set; }

    public DateTime NgayCapNhat { get; set; }

    public SanPham SanPham { get; set; } = null!;
    public KhiaCanh KhiaCanh { get; set; } = null!;
}
