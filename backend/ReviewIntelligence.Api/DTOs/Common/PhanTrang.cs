using System.ComponentModel.DataAnnotations;

namespace ReviewIntelligence.Api.DTOs.Common;

/// <summary>Tham số phân trang chung (query string: ?trang=1&amp;kichThuoc=12).</summary>
public class PhanTrangQuery
{
    [Range(1, 100_000, ErrorMessage = "Trang phải từ 1 trở lên.")]
    public int Trang { get; init; } = 1;

    [Range(1, 50, ErrorMessage = "Kích thước trang phải từ 1 đến 50.")]
    public int KichThuoc { get; init; } = 12;

    public int BoQua => (Trang - 1) * KichThuoc;
}

/// <summary>Kết quả có phân trang.</summary>
public record KetQuaPhanTrang<T>(IReadOnlyList<T> DuLieu, int TongSo, int Trang, int KichThuoc)
{
    public int TongSoTrang => KichThuoc <= 0 ? 0 : (int)Math.Ceiling(TongSo / (double)KichThuoc);
}
