namespace ReviewIntelligence.Api.Services;

/// <summary>Quy tắc giá bán của BienThe (giờ UTC, khớp SYSUTCDATETIME()).</summary>
public static class GiaBan
{
    /// <summary>GiaKhuyenMai chỉ có hiệu lực khi now ∈ [KhuyenMaiTu, KhuyenMaiDen].</summary>
    public static bool KhuyenMaiHieuLuc(decimal? giaKhuyenMai, DateTime? tu, DateTime? den, DateTime now) =>
        giaKhuyenMai is not null && tu <= now && den >= now;

    public static decimal ApDung(decimal giaBan, decimal? giaKhuyenMai, DateTime? tu, DateTime? den, DateTime now) =>
        KhuyenMaiHieuLuc(giaKhuyenMai, tu, den, now) ? giaKhuyenMai!.Value : giaBan;
}
