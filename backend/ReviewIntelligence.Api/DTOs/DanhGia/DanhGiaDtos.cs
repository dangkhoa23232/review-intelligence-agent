using System.ComponentModel.DataAnnotations;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Validation;

namespace ReviewIntelligence.Api.DTOs.DanhGia;

// ===================== Request =====================

/// <summary>Điểm 1 tiêu chí (dbo.DiemTieuChi).</summary>
public class DiemTieuChiRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã khía cạnh không hợp lệ.")]
    public int MaKhiaCanh { get; init; }

    /// <summary>CK_DiemTieuChi_Diem: 1..5.</summary>
    [Range(1, 5, ErrorMessage = "Điểm tiêu chí phải từ 1 đến 5.")]
    public byte Diem { get; init; }
}

/// <summary>Body của POST /api/danh-gia.</summary>
public class TaoDanhGiaRequest : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Mã sản phẩm không hợp lệ.")]
    public int MaSanPham { get; init; }

    /// <summary>CK_DanhGia_SoSao: 1..5.</summary>
    [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
    public byte SoSao { get; init; }

    /// <summary>dbo.DanhGia.NoiDung NVARCHAR(2000) NOT NULL.</summary>
    [Required(ErrorMessage = "Nội dung đánh giá là bắt buộc.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Nội dung đánh giá phải từ 10 đến 2000 ký tự.")]
    public string NoiDung { get; init; } = string.Empty;

    /// <summary>Dòng đơn hàng đã mua (tùy chọn) – hợp lệ thì DaMuaHang = true.</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Mã chi tiết đơn hàng không hợp lệ.")]
    public int? MaChiTietDH { get; init; }

    [MaxLength(20, ErrorMessage = "Tối đa 20 tiêu chí.")]
    public List<DiemTieuChiRequest> DiemTieuChi { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (string.IsNullOrWhiteSpace(NoiDung))
            yield return new ValidationResult("Nội dung đánh giá không được để trống.", [nameof(NoiDung)]);

        // PK_DiemTieuChi (MaDanhGia, MaKhiaCanh) – mỗi khía cạnh chỉ chấm 1 lần.
        if (DiemTieuChi.GroupBy(x => x.MaKhiaCanh).Any(g => g.Count() > 1))
            yield return new ValidationResult("Mỗi khía cạnh chỉ được chấm điểm một lần.", [nameof(DiemTieuChi)]);
    }
}

/// <summary>Body của PUT /api/admin/danh-gia/{maDanhGia}/trang-thai.</summary>
public class DoiTrangThaiDanhGiaRequest
{
    /// <summary>Chỉ cho phép chuyển sang DaDuyet hoặc DaAn (tập con CK_DanhGia_TrangThai).</summary>
    [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
    [ThuocTapGiaTri(TrangThaiDanhGia.DaDuyet, TrangThaiDanhGia.DaAn,
        ErrorMessage = "Trạng thái chỉ được là DaDuyet hoặc DaAn.")]
    public string TrangThai { get; init; } = string.Empty;
}

/// <summary>Body của POST /api/danh-gia/{maDanhGia}/bao-cao.</summary>
public class BaoCaoDanhGiaRequest
{
    /// <summary>dbo.BaoCaoDanhGia.LyDo NVARCHAR(300) NOT NULL.</summary>
    [Required(ErrorMessage = "Lý do báo cáo là bắt buộc.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "Lý do báo cáo phải từ 5 đến 300 ký tự.")]
    public string LyDo { get; init; } = string.Empty;
}

/// <summary>Query của GET /api/admin/danh-gia.</summary>
public class DanhGiaQuanTriQuery : PhanTrangQuery
{
    [ThuocTapGiaTri(TrangThaiDanhGia.ChoDuyet, TrangThaiDanhGia.DaDuyet, TrangThaiDanhGia.DaAn)]
    public string? TrangThai { get; init; }
}

// ===================== Response =====================

public record DiemTieuChiDto(int MaKhiaCanh, string TenKhiaCanh, byte Diem);

/// <summary>Đánh giá hiển thị công khai (chỉ TrangThai = DaDuyet).</summary>
public record DanhGiaDto(
    int MaDanhGia,
    byte SoSao,
    string NoiDung,
    bool DaMuaHang,
    string? CamXuc,
    DateTime NgayTao,
    string TenNguoiDanhGia,
    string? DungTichDaMua,
    IReadOnlyList<DiemTieuChiDto> DiemTieuChi);

public record DanhGiaDaTaoDto(int MaDanhGia, int MaSanPham, string TrangThai, bool DaMuaHang, DateTime NgayTao);

/// <summary>Dòng đơn hàng HoanTat của người dùng chưa được đánh giá – dùng cho form đánh giá.</summary>
public record DongDonCoTheDanhGiaDto(int MaChiTietDH, int MaDonHang, string DungTich, DateTime NgayDat);

public record DanhGiaQuanTriDto(
    int MaDanhGia,
    int MaSanPham,
    string TenSanPham,
    string DuongDanSanPham,
    int MaNguoiDung,
    string TenNguoiDung,
    string Email,
    byte SoSao,
    string NoiDung,
    bool DaMuaHang,
    string TrangThai,
    string? CamXuc,
    DateTime NgayTao,
    DateTime? NgayCapNhat,
    int SoBaoCaoChoXuLy,
    IReadOnlyList<DiemTieuChiDto> DiemTieuChi);

public record DanhSachDanhGiaQuanTriDto(
    KetQuaPhanTrang<DanhGiaQuanTriDto> KetQua,
    IReadOnlyDictionary<string, int> SoLuongTheoTrangThai);

public record ThongKeSanPhamDto(int MaSanPham, decimal DiemTrungBinh, int SoDanhGia);

public record DoiTrangThaiDanhGiaResponse(
    int MaDanhGia,
    string TrangThaiCu,
    string TrangThai,
    ThongKeSanPhamDto SanPham,
    bool DaGuiPhanTichAi,
    bool DaGuiGoBoVector);

public record BaoCaoDanhGiaDto(int MaBaoCao, int MaDanhGia, string LyDo, string TrangThai, DateTime NgayTao);
