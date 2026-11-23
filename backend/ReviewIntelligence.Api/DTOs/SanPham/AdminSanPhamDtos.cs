using System.ComponentModel.DataAnnotations;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.TraCuu;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Validation;

namespace ReviewIntelligence.Api.DTOs.SanPham;

// ======================================================================= Truy vấn

/// <summary>Query của GET /api/admin/san-pham.</summary>
public class SanPhamQuanTriQuery : PhanTrangQuery
{
    [MaxLength(100, ErrorMessage = "Từ khóa tối đa 100 ký tự.")]
    public string? TuKhoa { get; init; }

    /// <summary>Bỏ trống = tất cả trạng thái.</summary>
    [ThuocTapGiaTri(TrangThaiSanPham.DangBan, TrangThaiSanPham.NgungBan)]
    public string? TrangThai { get; init; }

    [Range(1, int.MaxValue)] public int? MaDanhMuc { get; init; }
    [Range(1, int.MaxValue)] public int? MaThuongHieu { get; init; }
}

// ======================================================================= Ghi

/// <summary>Body của POST /api/admin/san-pham và PUT /api/admin/san-pham/{maSanPham}.</summary>
public class LuuSanPhamRequest : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn danh mục.")]
    public int MaDanhMuc { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Vui lòng chọn thương hiệu.")]
    public int MaThuongHieu { get; init; }

    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc.")]
    [StringLength(255, MinimumLength = 3, ErrorMessage = "Tên sản phẩm từ 3 đến 255 ký tự.")]
    public string Ten { get; init; } = "";

    /// <summary>Bỏ trống → tự sinh từ tên (bỏ dấu, kebab-case).</summary>
    [MaxLength(280, ErrorMessage = "Đường dẫn tối đa 280 ký tự.")]
    [RegularExpression("^[a-z0-9]+(-[a-z0-9]+)*$", ErrorMessage = "Đường dẫn chỉ gồm chữ thường không dấu, số và dấu gạch ngang.")]
    public string? DuongDan { get; init; }

    [MaxLength(8000, ErrorMessage = "Mô tả tối đa 8000 ký tự.")]
    public string? MoTa { get; init; }

    [MaxLength(300, ErrorMessage = "Công dụng tối đa 300 ký tự.")]
    public string? CongDung { get; init; }

    [MaxLength(4000, ErrorMessage = "Thành phần tối đa 4000 ký tự.")]
    public string? ThanhPhan { get; init; }

    [MaxLength(1000, ErrorMessage = "Hướng dẫn sử dụng tối đa 1000 ký tự.")]
    public string? HuongDanSuDung { get; init; }

    /// <summary>Đường dẫn ảnh theo thứ tự hiển thị; ảnh đầu là ảnh đại diện.</summary>
    [MaxLength(10, ErrorMessage = "Tối đa 10 ảnh.")]
    public List<string> DanhSachAnh { get; init; } = [];

    [MaxLength(10)]
    public List<int> MaLoaiDa { get; init; } = [];

    [ThuocTapGiaTri(TrangThaiSanPham.DangBan, TrangThaiSanPham.NgungBan)]
    public string TrangThai { get; init; } = TrangThaiSanPham.DangBan;

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        for (var i = 0; i < DanhSachAnh.Count; i++)
        {
            var a = DanhSachAnh[i];
            var hopLe = !string.IsNullOrWhiteSpace(a) && a.Length <= 500
                && (a.StartsWith('/') && !a.StartsWith("//") && !a.Contains("..")
                    || Uri.TryCreate(a, UriKind.Absolute, out var u) && (u.Scheme == Uri.UriSchemeHttps || u.Scheme == Uri.UriSchemeHttp));
            if (!hopLe)
                yield return new ValidationResult($"Ảnh thứ {i + 1} không hợp lệ (cần đường dẫn bắt đầu bằng '/' hoặc URL http/https).", [nameof(DanhSachAnh)]);
        }
        if (DanhSachAnh.Distinct(StringComparer.OrdinalIgnoreCase).Count() != DanhSachAnh.Count)
            yield return new ValidationResult("Danh sách ảnh bị trùng.", [nameof(DanhSachAnh)]);
        if (MaLoaiDa.Any(x => x <= 0))
            yield return new ValidationResult("Mã loại da không hợp lệ.", [nameof(MaLoaiDa)]);
    }
}

public class DoiTrangThaiSanPhamRequest
{
    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    [ThuocTapGiaTri(TrangThaiSanPham.DangBan, TrangThaiSanPham.NgungBan)]
    public string TrangThai { get; init; } = "";
}

/// <summary>Body tạo / sửa biến thể. SoLuongTon không sửa trực tiếp – chỉ tăng qua nhập lô hàng.</summary>
public class LuuBienTheRequest : IValidatableObject
{
    [Required(ErrorMessage = "Mã SKU là bắt buộc.")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Mã SKU từ 2 đến 50 ký tự.")]
    [RegularExpression("^[A-Za-z0-9][A-Za-z0-9._-]*$", ErrorMessage = "Mã SKU chỉ gồm chữ không dấu, số, '.', '_' và '-'.")]
    public string MaSKU { get; init; } = "";

    [Required(ErrorMessage = "Dung tích là bắt buộc.")]
    [StringLength(30, MinimumLength = 1, ErrorMessage = "Dung tích tối đa 30 ký tự.")]
    public string DungTich { get; init; } = "";

    [Range(0, 1_000_000_000, ErrorMessage = "Giá bán không hợp lệ.")]
    public decimal GiaBan { get; init; }

    [Range(0, 1_000_000_000, ErrorMessage = "Giá khuyến mãi không hợp lệ.")]
    public decimal? GiaKhuyenMai { get; init; }

    public DateTime? KhuyenMaiTu { get; init; }
    public DateTime? KhuyenMaiDen { get; init; }

    public bool DangHoatDong { get; init; } = true;

    /// <summary>Đồng bộ CK_BienThe_1: GiaKhuyenMai &lt; GiaBan và KhuyenMaiDen &gt; KhuyenMaiTu.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (GiaKhuyenMai is null)
        {
            if (KhuyenMaiTu is not null || KhuyenMaiDen is not null)
                yield return new ValidationResult("Chưa nhập giá khuyến mãi nhưng lại có thời gian khuyến mãi.", [nameof(GiaKhuyenMai)]);
            yield break;
        }
        if (GiaKhuyenMai >= GiaBan)
            yield return new ValidationResult("Giá khuyến mãi phải nhỏ hơn giá bán.", [nameof(GiaKhuyenMai)]);
        if (KhuyenMaiTu is null || KhuyenMaiDen is null)
            yield return new ValidationResult("Cần nhập thời gian bắt đầu và kết thúc khuyến mãi.", [nameof(KhuyenMaiTu), nameof(KhuyenMaiDen)]);
        else if (KhuyenMaiDen <= KhuyenMaiTu)
            yield return new ValidationResult("Thời gian kết thúc khuyến mãi phải sau thời gian bắt đầu.", [nameof(KhuyenMaiDen)]);
    }
}

/// <summary>Body nhập lô hàng: SoLuongConLai = SoLuongNhap, BienThe.SoLuongTon += SoLuongNhap.</summary>
public class NhapLoHangRequest
{
    [Required(ErrorMessage = "Số lô là bắt buộc.")]
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Số lô tối đa 50 ký tự.")]
    [RegularExpression("^[A-Za-z0-9][A-Za-z0-9._/-]*$", ErrorMessage = "Số lô chỉ gồm chữ không dấu, số, '.', '_', '/' và '-'.")]
    public string SoLo { get; init; } = "";

    [Required(ErrorMessage = "Hạn sử dụng là bắt buộc.")]
    public DateOnly? HanSuDung { get; init; }

    [Range(1, 100_000, ErrorMessage = "Số lượng nhập từ 1 đến 100.000.")]
    public int SoLuongNhap { get; init; }
}

// ======================================================================= Kết quả

public record SanPhamQuanTriDto(
    int MaSanPham,
    string Ten,
    string DuongDan,
    string? AnhDaiDien,
    string TenThuongHieu,
    string TenDanhMuc,
    string TrangThai,
    int SoBienThe,
    int TongTon,
    decimal? GiaThapNhat,
    decimal DiemTrungBinh,
    int SoDanhGia,
    DateTime NgayTao);

public record DanhSachSanPhamQuanTriDto(
    KetQuaPhanTrang<SanPhamQuanTriDto> KetQua,
    IReadOnlyDictionary<string, int> SoLuongTheoTrangThai);

public record LoHangDto(
    int MaLo,
    string SoLo,
    DateOnly HanSuDung,
    int SoLuongNhap,
    int SoLuongConLai,
    DateTime NgayNhap);

public record BienTheQuanTriDto(
    int MaBienThe,
    string MaSKU,
    string DungTich,
    decimal GiaBan,
    decimal? GiaKhuyenMai,
    DateTime? KhuyenMaiTu,
    DateTime? KhuyenMaiDen,
    int SoLuongTon,
    bool DangHoatDong,
    IReadOnlyList<LoHangDto> LoHang);

public record SanPhamQuanTriChiTietDto(
    int MaSanPham,
    int MaDanhMuc,
    int MaThuongHieu,
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
    DateTime NgayTao,
    IReadOnlyList<LoaiDaDto> LoaiDa,
    IReadOnlyList<BienTheQuanTriDto> BienThe);

public record AnhDaTaiLenDto(string DuongDan, string TenTep, long KichThuoc);
