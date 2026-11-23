using System.ComponentModel.DataAnnotations;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.DanhGia;
using ReviewIntelligence.Api.DTOs.TraCuu;
using ReviewIntelligence.Api.Validation;

namespace ReviewIntelligence.Api.DTOs.SanPham;

/// <summary>Giá trị hợp lệ cho tham số sapXep của GET /api/san-pham.</summary>
public static class SapXepSanPham
{
    public const string MoiNhat = "moiNhat";
    public const string GiaTang = "giaTang";
    public const string GiaGiam = "giaGiam";
    public const string DanhGiaCao = "danhGiaCao";
}

/// <summary>Query của GET /api/san-pham.</summary>
public class TimKiemSanPhamQuery : PhanTrangQuery, IValidatableObject
{
    /// <summary>Tìm trong Ten và CongDung.</summary>
    [MaxLength(100, ErrorMessage = "Từ khóa tối đa 100 ký tự.")]
    public string? TuKhoa { get; init; }

    /// <summary>Lọc theo danh mục (bao gồm danh mục con trực tiếp).</summary>
    [Range(1, int.MaxValue)] public int? MaDanhMuc { get; init; }
    [Range(1, int.MaxValue)] public int? MaThuongHieu { get; init; }
    [Range(1, int.MaxValue)] public int? MaLoaiDa { get; init; }

    /// <summary>Lọc theo giá thấp nhất đang áp dụng (đã tính giá khuyến mãi còn hiệu lực).</summary>
    [Range(0, 1_000_000_000, ErrorMessage = "Giá từ không hợp lệ.")]
    public decimal? GiaTu { get; init; }

    [Range(0, 1_000_000_000, ErrorMessage = "Giá đến không hợp lệ.")]
    public decimal? GiaDen { get; init; }

    [ThuocTapGiaTri(SapXepSanPham.MoiNhat, SapXepSanPham.GiaTang, SapXepSanPham.GiaGiam, SapXepSanPham.DanhGiaCao)]
    public string SapXep { get; init; } = SapXepSanPham.MoiNhat;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (GiaTu is { } tu && GiaDen is { } den && tu > den)
            yield return new ValidationResult("Giá từ phải nhỏ hơn hoặc bằng giá đến.", [nameof(GiaTu), nameof(GiaDen)]);
    }
}

/// <summary>Thẻ sản phẩm trong danh sách.</summary>
public record SanPhamTomTatDto(
    int MaSanPham,
    string Ten,
    string DuongDan,
    string? CongDung,
    string? AnhDaiDien,
    string TenThuongHieu,
    string TenDanhMuc,
    decimal? GiaThapNhat,
    bool CoKhuyenMai,
    bool ConHang,
    decimal DiemTrungBinh,
    int SoDanhGia)
{
    /// <summary>Giá bán gốc (gạch ngang) của biến thể có GiaThapNhat khi biến thể đó đang khuyến mãi; ngược lại null.</summary>
    public decimal? GiaGoc { get; init; }

    /// <summary>Số biến thể đang hoạt động.</summary>
    public int SoBienThe { get; init; }

    /// <summary>Biến thể dùng cho nút "Thêm vào giỏ" trên thẻ: còn hàng và rẻ nhất; null nếu chưa có biến thể.</summary>
    public BienTheMacDinhDto? BienTheMacDinh { get; init; }
}

public record BienTheMacDinhDto(int MaBienThe, string DungTich, decimal GiaApDung, decimal? GiaGoc, int SoLuongTon);

public record DanhMucRutGonDto(int MaDanhMuc, string Ten, string DuongDan);

/// <summary>dbo.BienThe – GiaKhuyenMai chỉ khác NULL khi đang trong khoảng KhuyenMaiTu..KhuyenMaiDen.</summary>
public record BienTheDto(
    int MaBienThe,
    string MaSKU,
    string DungTich,
    decimal GiaBan,
    decimal? GiaKhuyenMai,
    DateTime? KhuyenMaiDen,
    decimal GiaApDung,
    int SoLuongTon,
    bool ConHang);

/// <summary>dbo.TomTatKhiaCanh – DanChung là danh sách MaDanhGia làm dẫn chứng.</summary>
public record TomTatKhiaCanhDto(
    int MaKhiaCanh,
    string TenKhiaCanh,
    int SoTichCuc,
    int SoTieuCuc,
    string? UuDiem,
    string? NhuocDiem,
    string? VanDeLapLai,
    IReadOnlyList<int> DanChung,
    DateTime NgayCapNhat);

public record SanPhamChiTietDto(
    int MaSanPham,
    string Ten,
    string DuongDan,
    string? MoTa,
    string? CongDung,
    string? ThanhPhan,
    string? HuongDanSuDung,
    IReadOnlyList<string> DanhSachAnh,
    string TrangThai,
    decimal DiemTrungBinh,
    int SoDanhGia,
    DanhMucRutGonDto DanhMuc,
    ThuongHieuDto ThuongHieu,
    IReadOnlyList<LoaiDaDto> LoaiDaPhuHop,
    IReadOnlyList<BienTheDto> BienThe,
    IReadOnlyList<TomTatKhiaCanhDto> TomTatKhiaCanh,
    KetQuaPhanTrang<DanhGiaDto> DanhGia);
