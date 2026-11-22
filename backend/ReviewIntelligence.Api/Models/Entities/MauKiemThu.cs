namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.MauKiemThu – mẫu trong bộ kiểm thử (test set) có nhãn chuẩn.</summary>
public class MauKiemThu
{
    public int MaMau { get; set; }
    public int? MaDanhGia { get; set; }
    public string NoiDung { get; set; } = null!;

    /// <summary>Xem <see cref="CamXuc"/>.</summary>
    public string CamXucChuan { get; set; } = null!;

    /// <summary>Chuỗi JSON (CK_MauKiemThu_KhiaCanhChuan).</summary>
    public string? KhiaCanhChuan { get; set; }

    /// <summary>Xem <see cref="Entities.TapDuLieu"/>.</summary>
    public string TapDuLieu { get; set; } = Entities.TapDuLieu.KiemThu;

    public DanhGia? DanhGia { get; set; }
    public ICollection<KetQuaThucNghiem> KetQuaThucNghiems { get; set; } = new List<KetQuaThucNghiem>();
}
