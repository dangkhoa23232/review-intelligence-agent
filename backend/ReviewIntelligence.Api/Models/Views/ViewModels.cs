namespace ReviewIntelligence.Api.Models.Views;

// Keyless entity ánh xạ các VIEW trong database/ReviewIntelligence_SQLServer.sql (chỉ đọc).
// Tên lớp = tên view bỏ tiền tố "vw_"; tên thuộc tính = tên cột của view.

/// <summary>dbo.vw_DoanhThuTheoNgay – doanh thu đơn HoanTat theo ngày.</summary>
public class DoanhThuTheoNgay
{
    public DateOnly Ngay { get; set; }
    public int SoDonHang { get; set; }
    public decimal DoanhThu { get; set; }
}

/// <summary>dbo.vw_SanPhamBanChay – số lượng và doanh thu theo sản phẩm (đơn HoanTat).</summary>
public class SanPhamBanChay
{
    public int MaSanPham { get; set; }
    public string Ten { get; set; } = null!;
    public int SoLuongBan { get; set; }
    public decimal DoanhThu { get; set; }
}

/// <summary>dbo.vw_TonKhoThap – biến thể tồn ≤ 10 hoặc có lô còn hàng hết hạn trong ≤ 90 ngày.</summary>
public class TonKhoThap
{
    public int MaBienThe { get; set; }
    public string MaSKU { get; set; } = null!;
    public string TenSanPham { get; set; } = null!;
    public int SoLuongTon { get; set; }
    public DateOnly? HanSuDungGanNhat { get; set; }
}

/// <summary>dbo.vw_ThongKeKhiaCanh – số ý kiến theo sản phẩm × khía cạnh (công cụ của tác tử).</summary>
public class ThongKeKhiaCanh
{
    public int MaSanPham { get; set; }
    public int MaKhiaCanh { get; set; }
    public string TenKhiaCanh { get; set; } = null!;
    public int SoTichCuc { get; set; }
    public int SoTrungTinh { get; set; }
    public int SoTieuCuc { get; set; }
}

/// <summary>dbo.vw_XuHuongCamXuc – xu hướng cảm xúc theo sản phẩm theo ngày.</summary>
public class XuHuongCamXuc
{
    public int MaSanPham { get; set; }
    public DateOnly Ngay { get; set; }
    public int SoTichCuc { get; set; }
    public int SoTrungTinh { get; set; }
    public int SoTieuCuc { get; set; }
}

/// <summary>dbo.vw_ChiSoThucNghiem – accuracy, precision/recall/F1 (macro), độ trễ theo mô hình.</summary>
public class ChiSoThucNghiem
{
    public string MoHinh { get; set; } = null!;
    public int SoMau { get; set; }
    public double DoChinhXac { get; set; }
    public double DoChuanXacMacro { get; set; }
    public double DoPhuMacro { get; set; }
    public double DiemF1Macro { get; set; }

    /// <summary>NULL nếu mọi mẫu đều không ghi DoTreMs.</summary>
    public double? DoTreTrungBinhMs { get; set; }
}
