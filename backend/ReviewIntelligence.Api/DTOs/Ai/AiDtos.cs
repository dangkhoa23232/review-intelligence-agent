using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.DTOs.Ai;

// ===================== Hỏi đáp RAG =====================

/// <summary>Body của POST /api/hoi-dap.</summary>
public class HoiDapRequest
{
    [Required(ErrorMessage = "Câu hỏi là bắt buộc.")]
    [StringLength(500, MinimumLength = 2, ErrorMessage = "Câu hỏi phải từ 2 đến 500 ký tự.")]
    public string CauHoi { get; init; } = string.Empty;

    /// <summary>Sản phẩm đang hỏi (null = hỏi chung trên mọi sản phẩm).</summary>
    [Range(1, int.MaxValue, ErrorMessage = "Mã sản phẩm không hợp lệ.")]
    public int? MaSanPham { get; init; }

    /// <summary>TinNhan.MaPhien – gửi lại để tiếp tục cùng phiên hội thoại.</summary>
    public Guid? MaPhien { get; init; }
}

public record DanChungDto(
    int MaDanhGia, string DoanTrich, double Diem, int SoSao, bool DaMuaHang, string TenNguoiDanhGia, string TenSanPham);

/// <summary>Một bước tác tử (dbo.NhatKyTacTu).</summary>
public record BuocTacTuDto(int BuocSo, string LoaiBuoc, string? TenCongCu, JsonElement? DauVao, JsonElement? DauRa, int? DoTreMs);

public record HoiDapResponse(
    Guid MaPhien,
    long MaTinNhanHoi,
    long MaTinNhan,
    string TraLoi,
    // tra_loi | thieu_dan_chung | tu_choi | loi
    string CheDo,
    // llm (Claude) | mau (câu trả lời mẫu từ dẫn chứng)
    string SinhBang,
    IReadOnlyList<DanChungDto> DanChung,
    IReadOnlyList<BuocTacTuDto> Buoc,
    int DoTreMs);

/// <summary>dbo.TinNhan trong một phiên.</summary>
public record TinNhanDto(long MaTinNhan, string NguoiGui, string NoiDung, JsonElement? DanChung, int? DoTreMs, DateTime NgayTao);

public record PhienHoiDapDto(Guid MaPhien, int? MaSanPham, IReadOnlyList<TinNhanDto> TinNhan);

// ===================== Trace tác tử (quản trị) =====================

public class TraceQuery : PhanTrangQuery
{
    [Range(1, int.MaxValue)] public int? MaSanPham { get; init; }
}

public record TraLoiTomTatDto(
    long MaTinNhan, Guid MaPhien, string? CauHoi, string TraLoi, int? MaSanPham, string? TenSanPham,
    string? TenNguoiDung, int SoBuoc, bool CoLoi, int SoDanChung, int? DoTreMs, DateTime NgayTao);

public record TraceChiTietDto(TraLoiTomTatDto TinNhan, JsonElement? DanChung, IReadOnlyList<BuocTacTuDto> Buoc);

// ===================== Thống kê (vw_ThongKeKhiaCanh, vw_XuHuongCamXuc) =====================

public record ThongKeKhiaCanhDto(
    int MaKhiaCanh, string TenKhiaCanh, int SoTichCuc, int SoTrungTinh, int SoTieuCuc);

public record XuHuongCamXucDto(DateOnly Ngay, int SoTichCuc, int SoTrungTinh, int SoTieuCuc);

public record BangDieuKhienDto(
    int? MaSanPham, int SoNgay, IReadOnlyList<ThongKeKhiaCanhDto> KhiaCanh, IReadOnlyList<XuHuongCamXucDto> XuHuong);

// ===================== Thực nghiệm (MauKiemThu, KetQuaThucNghiem, vw_ChiSoThucNghiem) =====================

public class ChayThucNghiemRequest : IValidatableObject
{
    [Required, MinLength(1, ErrorMessage = "Chọn ít nhất một mô hình.")]
    public List<string> MoHinh { get; init; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var sai = MoHinh.Where(m => !MoHinhAbsa.TatCa.Contains(m)).ToList();
        if (sai.Count > 0)
            yield return new ValidationResult(
                $"Mô hình không hợp lệ: {string.Join(", ", sai)}. Chấp nhận: {string.Join(", ", MoHinhAbsa.TatCa)}.",
                [nameof(MoHinh)]);
    }
}

/// <summary>Chỉ số khía cạnh (micro) tính từ KhiaCanhChuan / KhiaCanhDuDoan – bổ sung cho view chỉ đo cảm xúc tổng.</summary>
public record ChiSoKhiaCanhDto(double DoChuanXac, double DoPhu, double DiemF1, double DoChinhXacCamXuc, int SoCapChuan);

public record ChiSoMoHinhDto(
    string MoHinh, int SoMau, double DoChinhXac, double DoChuanXacMacro, double DoPhuMacro, double DiemF1Macro,
    double? DoTreTrungBinhMs, ChiSoKhiaCanhDto KhiaCanh, DateTime? NgayChayGanNhat)
{
    /// <summary>true khi đã chạy đủ toàn bộ mẫu KiemThu – chỉ khi đó số liệu mới dùng để so sánh.</summary>
    public bool DuMau { get; init; }
    public double? DoTreTrungViMs { get; init; }
    public double? DoTreP95Ms { get; init; }
    public IReadOnlyList<ChiSoLopDto> TheoLop { get; init; } = [];

    /// <summary>Ô ma trận nhầm lẫn (nhãn chuẩn × nhãn dự đoán), đủ 9 ô.</summary>
    public IReadOnlyList<ONhamLanDto> MaTranNhamLan { get; init; } = [];

    /// <summary>Nhóm lỗi (gộp từ KetQuaThucNghiem.NhomLoi) – nhiều nhất trước.</summary>
    public IReadOnlyList<NhomLoiDto> NhomLoi { get; init; } = [];
}

public record ChiSoLopDto(string Nhan, double DoChuanXac, double DoPhu, double DiemF1, int SoMauChuan);

public record ONhamLanDto(string Chuan, string DuDoan, int SoMau);

public record ChiTietNhomLoiDto(string NhomLoi, int SoMau);

public record ViDuLoiDto(int MaMau, string NoiDung, string CamXucChuan, string CamXucDuDoan);

public record NhomLoiDto(
    string NhomChinh, int SoMau, double TiLeTrenMauSai,
    IReadOnlyList<ChiTietNhomLoiDto> ChiTiet, IReadOnlyList<ViDuLoiDto> ViDu);

public record ThongTinBoMauDto(int SoKiemThu, int SoViDuMau, IReadOnlyDictionary<string, int> PhanBoNhan);

public record ChiSoThucNghiemDto(ThongTinBoMauDto BoMau, IReadOnlyList<ChiSoMoHinhDto> MoHinh);

public class KetQuaThucNghiemQuery : PhanTrangQuery
{
    [Required]
    [Validation.ThuocTapGiaTri(MoHinhAbsa.PhoBERT, MoHinhAbsa.LLM_KhongVD, MoHinhAbsa.LLM_ItVD)]
    public string MoHinh { get; init; } = MoHinhAbsa.PhoBERT;

    /// <summary>true: chỉ mẫu dự đoán sai.</summary>
    public bool ChiSai { get; init; }
}

public record KetQuaMauDto(
    int MaMau, string NoiDung, string CamXucChuan, string CamXucDuDoan,
    IReadOnlyDictionary<string, string> KhiaCanhChuan, IReadOnlyDictionary<string, string> KhiaCanhDuDoan,
    bool LaDung, string? NhomLoi, int? DoTreMs, DateTime NgayChay);
