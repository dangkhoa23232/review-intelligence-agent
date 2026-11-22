namespace ReviewIntelligence.Api.Models.Entities;

/// <summary>
/// Bảng dbo.DanhGia – đánh giá sản phẩm.
/// Có trigger dbo.trg_DanhGia_CapNhatDiem (AFTER INSERT/UPDATE/DELETE) cập nhật
/// SanPham.DiemTrungBinh và SanPham.SoDanhGia.
/// </summary>
public class DanhGia
{
    public int MaDanhGia { get; set; }
    public int MaSanPham { get; set; }
    public int MaNguoiDung { get; set; }
    public int? MaChiTietDH { get; set; }

    /// <summary>1–5 (CK_DanhGia_SoSao).</summary>
    public byte SoSao { get; set; }

    public string NoiDung { get; set; } = null!;
    public bool DaMuaHang { get; set; }

    /// <summary>Xem <see cref="TrangThaiDanhGia"/>.</summary>
    public string TrangThai { get; set; } = TrangThaiDanhGia.ChoDuyet;

    /// <summary>Xem <see cref="Entities.CamXuc"/>; NULL khi chưa phân tích.</summary>
    public string? CamXuc { get; set; }

    /// <summary>0–1 (CK_DanhGia_DoTinCay).</summary>
    public decimal? DoTinCay { get; set; }

    /// <summary>ID điểm (point id) của review trong Qdrant.</summary>
    public Guid? MaVectorQdrant { get; set; }

    public DateTime NgayTao { get; set; }
    public DateTime? NgayCapNhat { get; set; }

    public SanPham SanPham { get; set; } = null!;
    public NguoiDung NguoiDung { get; set; } = null!;
    public ChiTietDonHang? ChiTietDonHang { get; set; }
    public ICollection<DiemTieuChi> DiemTieuChis { get; set; } = new List<DiemTieuChi>();
    public ICollection<BaoCaoDanhGia> BaoCaoDanhGias { get; set; } = new List<BaoCaoDanhGia>();
    public ICollection<KetQuaKhiaCanh> KetQuaKhiaCanhs { get; set; } = new List<KetQuaKhiaCanh>();
}
