using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Services;

/// <summary>Phí vận chuyển cố định theo DonHang.PhuongThucGiaoHang.</summary>
public static class BangPhiVanChuyen
{
    public static readonly IReadOnlyDictionary<string, decimal> TheoPhuongThuc = new Dictionary<string, decimal>
    {
        [PhuongThucGiaoHang.TieuChuan] = 25_000m,
        [PhuongThucGiaoHang.Nhanh] = 45_000m
    };

    public static decimal Tinh(string phuongThucGiaoHang) => TheoPhuongThuc[phuongThucGiaoHang];
}
