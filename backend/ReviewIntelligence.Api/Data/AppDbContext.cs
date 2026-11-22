using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Models.Views;

namespace ReviewIntelligence.Api.Data;

/// <summary>
/// DbContext ánh xạ tới ReviewIntelligenceDB (database-first).
/// Schema gốc: database/ReviewIntelligence_SQLServer.sql – KHÔNG dùng EF migrations để tạo/sửa bảng.
/// Tên bảng/cột khớp chính xác schema; cấu hình chi tiết nằm trong Data/Configurations.
/// Thủ tục lưu trữ: xem AppDbContext.StoredProcedures.cs.
/// </summary>
public partial class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ----- Tài khoản -----
    public DbSet<LoaiDa> LoaiDa => Set<LoaiDa>();
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();

    // ----- Sản phẩm & tồn kho -----
    public DbSet<DanhMuc> DanhMuc => Set<DanhMuc>();
    public DbSet<ThuongHieu> ThuongHieu => Set<ThuongHieu>();
    public DbSet<SanPham> SanPham => Set<SanPham>();
    public DbSet<SanPhamLoaiDa> SanPhamLoaiDa => Set<SanPhamLoaiDa>();
    public DbSet<BienThe> BienThe => Set<BienThe>();
    public DbSet<LoHang> LoHang => Set<LoHang>();
    public DbSet<BoChamSoc> BoChamSoc => Set<BoChamSoc>();
    public DbSet<ChiTietBoChamSoc> ChiTietBoChamSoc => Set<ChiTietBoChamSoc>();
    public DbSet<YeuThich> YeuThich => Set<YeuThich>();

    // ----- Giỏ hàng, đơn hàng & khuyến mãi -----
    public DbSet<GioHang> GioHang => Set<GioHang>();
    public DbSet<PhieuGiamGia> PhieuGiamGia => Set<PhieuGiamGia>();
    public DbSet<KhuyenMai> KhuyenMai => Set<KhuyenMai>();
    public DbSet<DonHang> DonHang => Set<DonHang>();
    public DbSet<ChiTietDonHang> ChiTietDonHang => Set<ChiTietDonHang>();

    // ----- Đánh giá -----
    public DbSet<KhiaCanh> KhiaCanh => Set<KhiaCanh>();
    public DbSet<DanhGia> DanhGia => Set<DanhGia>();
    public DbSet<DiemTieuChi> DiemTieuChi => Set<DiemTieuChi>();
    public DbSet<BaoCaoDanhGia> BaoCaoDanhGia => Set<BaoCaoDanhGia>();

    // ----- Thông báo & nhật ký -----
    public DbSet<ThongBao> ThongBao => Set<ThongBao>();
    public DbSet<NhatKyHeThong> NhatKyHeThong => Set<NhatKyHeThong>();

    // ----- Phân tích AI, tác tử & thực nghiệm -----
    public DbSet<KetQuaKhiaCanh> KetQuaKhiaCanh => Set<KetQuaKhiaCanh>();
    public DbSet<TomTatKhiaCanh> TomTatKhiaCanh => Set<TomTatKhiaCanh>();
    public DbSet<TinNhan> TinNhan => Set<TinNhan>();
    public DbSet<NhatKyTacTu> NhatKyTacTu => Set<NhatKyTacTu>();
    public DbSet<MauKiemThu> MauKiemThu => Set<MauKiemThu>();
    public DbSet<KetQuaThucNghiem> KetQuaThucNghiem => Set<KetQuaThucNghiem>();

    // ----- View (chỉ đọc) -----
    public DbSet<DoanhThuTheoNgay> VwDoanhThuTheoNgay => Set<DoanhThuTheoNgay>();
    public DbSet<SanPhamBanChay> VwSanPhamBanChay => Set<SanPhamBanChay>();
    public DbSet<TonKhoThap> VwTonKhoThap => Set<TonKhoThap>();
    public DbSet<ThongKeKhiaCanh> VwThongKeKhiaCanh => Set<ThongKeKhiaCanh>();
    public DbSet<XuHuongCamXuc> VwXuHuongCamXuc => Set<XuHuongCamXuc>();
    public DbSet<ChiSoThucNghiem> VwChiSoThucNghiem => Set<ChiSoThucNghiem>();

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        // Mọi cột thời gian trong schema đều là DATETIME2(0); cột DATE dùng DateOnly.
        builder.Properties<DateTime>().HaveColumnType("datetime2(0)");
        builder.Properties<DateTime?>().HaveColumnType("datetime2(0)");
        builder.Properties<DateOnly>().HaveColumnType("date");
        builder.Properties<DateOnly?>().HaveColumnType("date");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("dbo");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
