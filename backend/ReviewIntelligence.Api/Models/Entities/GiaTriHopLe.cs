namespace ReviewIntelligence.Api.Models.Entities;

// Các giá trị hợp lệ khớp CHECK constraint trong database/ReviewIntelligence_SQLServer.sql.
// Dùng các hằng này thay cho chuỗi "magic" trong code và validation DTO.

/// <summary>CK_SanPham_TrangThai.</summary>
public static class TrangThaiSanPham
{
    public const string DangBan = "DangBan";
    public const string NgungBan = "NgungBan";
    public static readonly string[] TatCa = [DangBan, NgungBan];
}

/// <summary>CK_PhieuGiamGia_KieuGiam.</summary>
public static class KieuGiam
{
    public const string PhanTram = "PhanTram";
    public const string SoTien = "SoTien";
    public static readonly string[] TatCa = [PhanTram, SoTien];
}

/// <summary>CK_DonHang_PhuongThucGiaoHang.</summary>
public static class PhuongThucGiaoHang
{
    public const string TieuChuan = "TieuChuan";
    public const string Nhanh = "Nhanh";
    public static readonly string[] TatCa = [TieuChuan, Nhanh];
}

/// <summary>CK_DonHang_PhuongThucThanhToan.</summary>
public static class PhuongThucThanhToan
{
    public const string TienMat = "TienMat";
    public const string ChuyenKhoanGiaLap = "ChuyenKhoanGiaLap";
    public static readonly string[] TatCa = [TienMat, ChuyenKhoanGiaLap];
}

/// <summary>CK_DonHang_TrangThai. Luồng chuyển do dbo.sp_DoiTrangThaiDonHang kiểm soát.</summary>
public static class TrangThaiDonHang
{
    public const string ChoXacNhan = "ChoXacNhan";
    public const string DaXacNhan = "DaXacNhan";
    public const string DangDongGoi = "DangDongGoi";
    public const string DangGiao = "DangGiao";
    public const string HoanTat = "HoanTat";
    public const string DaHuy = "DaHuy";
    public static readonly string[] TatCa = [ChoXacNhan, DaXacNhan, DangDongGoi, DangGiao, HoanTat, DaHuy];

    /// <summary>
    /// Trạng thái được phép chuyển tới – mô phỏng đúng luồng kiểm tra trong dbo.sp_DoiTrangThaiDonHang
    /// (thủ tục vẫn là nơi kiểm tra cuối cùng).
    /// </summary>
    public static IReadOnlyList<string> TiepTheo(string trangThai) => trangThai switch
    {
        ChoXacNhan => [DaXacNhan, DaHuy],
        DaXacNhan => [DangDongGoi, DaHuy],
        DangDongGoi => [DangGiao, DaHuy],
        DangGiao => [HoanTat],
        _ => []
    };
}

/// <summary>CK_DanhGia_TrangThai.</summary>
public static class TrangThaiDanhGia
{
    public const string ChoDuyet = "ChoDuyet";
    public const string DaDuyet = "DaDuyet";
    public const string DaAn = "DaAn";
    public static readonly string[] TatCa = [ChoDuyet, DaDuyet, DaAn];
}

/// <summary>CK_DanhGia_CamXuc, CK_KetQuaKhiaCanh_CamXuc, CK_MauKiemThu_CamXucChuan, CK_KetQuaThucNghiem_CamXucDuDoan.</summary>
public static class CamXuc
{
    public const string TichCuc = "TichCuc";
    public const string TrungTinh = "TrungTinh";
    public const string TieuCuc = "TieuCuc";
    public static readonly string[] TatCa = [TichCuc, TrungTinh, TieuCuc];
}

/// <summary>CK_BaoCaoDanhGia_TrangThai.</summary>
public static class TrangThaiBaoCao
{
    public const string ChoXuLy = "ChoXuLy";
    public const string DaXuLy = "DaXuLy";
    public const string BoQua = "BoQua";
    public static readonly string[] TatCa = [ChoXuLy, DaXuLy, BoQua];
}

/// <summary>CK_KetQuaKhiaCanh_MoHinh, CK_KetQuaThucNghiem_MoHinh.</summary>
public static class MoHinhAbsa
{
    public const string PhoBERT = "PhoBERT";
    public const string LLM_KhongVD = "LLM_KhongVD";
    public const string LLM_ItVD = "LLM_ItVD";
    public static readonly string[] TatCa = [PhoBERT, LLM_KhongVD, LLM_ItVD];
}

/// <summary>CK_TinNhan_NguoiGui.</summary>
public static class NguoiGui
{
    public const string NguoiDung = "NguoiDung";
    public const string TroLy = "TroLy";
    public static readonly string[] TatCa = [NguoiDung, TroLy];
}

/// <summary>CK_NhatKyTacTu_LoaiBuoc.</summary>
public static class LoaiBuocTacTu
{
    public const string LapKeHoach = "LapKeHoach";
    public const string GoiCongCu = "GoiCongCu";
    public const string QuanSat = "QuanSat";
    public const string KiemSoat = "KiemSoat";
    public const string TraLoi = "TraLoi";
    public const string Loi = "Loi";
    public static readonly string[] TatCa = [LapKeHoach, GoiCongCu, QuanSat, KiemSoat, TraLoi, Loi];
}

/// <summary>CK_MauKiemThu_TapDuLieu.</summary>
public static class TapDuLieu
{
    public const string KiemThu = "KiemThu";
    public const string ViDuMau = "ViDuMau";
    public static readonly string[] TatCa = [KiemThu, ViDuMau];
}
