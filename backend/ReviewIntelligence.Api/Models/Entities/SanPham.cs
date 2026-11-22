namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>Bảng dbo.SanPham – sản phẩm.</summary>
public class SanPham
{
    public int MaSanPham { get; set; }
    public int MaDanhMuc { get; set; }
    public int MaThuongHieu { get; set; }
    public string Ten { get; set; } = null!;
    public string DuongDan { get; set; } = null!;
    public string? MoTa { get; set; }
    public string? CongDung { get; set; }
    public string? ThanhPhan { get; set; }
    public string? HuongDanSuDung { get; set; }

    /// <summary>Chuỗi JSON (CK_SanPham_DanhSachAnh: ISJSON = 1).</summary>
    public string? DanhSachAnh { get; set; }

    /// <summary>Xem <see cref="TrangThaiSanPham"/>.</summary>
    public string TrangThai { get; set; } = TrangThaiSanPham.DangBan;

    /// <summary>Do trigger trg_DanhGia_CapNhatDiem tính – ứng dụng không ghi.</summary>
    public decimal DiemTrungBinh { get; set; }

    /// <summary>Do trigger trg_DanhGia_CapNhatDiem tính – ứng dụng không ghi.</summary>
    public int SoDanhGia { get; set; }

    public DateTime NgayTao { get; set; }

    public DanhMuc DanhMuc { get; set; } = null!;
    public ThuongHieu ThuongHieu { get; set; } = null!;
    public ICollection<SanPhamLoaiDa> SanPhamLoaiDas { get; set; } = new List<SanPhamLoaiDa>();
    public ICollection<BienThe> BienThes { get; set; } = new List<BienThe>();
    public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    public ICollection<TomTatKhiaCanh> TomTatKhiaCanhs { get; set; } = new List<TomTatKhiaCanh>();
    public ICollection<KhuyenMai> KhuyenMais { get; set; } = new List<KhuyenMai>();
}
