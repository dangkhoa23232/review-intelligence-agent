namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.NguoiDung – tài khoản người dùng.</summary>
public class NguoiDung
{
    public int MaNguoiDung { get; set; }
    public string Email { get; set; } = null!;
    public string MatKhauBam { get; set; } = null!;
    public string HoTen { get; set; } = null!;
    public string? SoDienThoai { get; set; }
    public string? DiaChi { get; set; }
    public int? MaLoaiDa { get; set; }

    /// <summary>'KhachHang' | 'QuanTri' (CK_NguoiDung_VaiTro).</summary>
    public string VaiTro { get; set; } = VaiTroNguoiDung.KhachHang;

    public string? MaDatLaiMatKhau { get; set; }
    public DateTime? HanMaDatLai { get; set; }
    public bool DangHoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; }

    public LoaiDa? LoaiDa { get; set; }
}

public static class VaiTroNguoiDung
{
    public const string KhachHang = "KhachHang";
    public const string QuanTri = "QuanTri";
}
