using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data.Configurations;

// Nhóm: Giỏ hàng, đơn hàng & khuyến mãi – GioHang, PhieuGiamGia, KhuyenMai, DonHang, ChiTietDonHang

public class GioHangConfiguration : IEntityTypeConfiguration<GioHang>
{
    public void Configure(EntityTypeBuilder<GioHang> e)
    {
        e.ToTable("GioHang", "dbo");
        e.HasKey(x => new { x.MaNguoiDung, x.MaBienThe }).HasName("PK_GioHang");
        e.Property(x => x.NgayThem).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasOne(x => x.NguoiDung).WithMany(x => x.GioHangs)
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_GioHang_MaNguoiDung")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.BienThe).WithMany()
            .HasForeignKey(x => x.MaBienThe)
            .HasConstraintName("FK_GioHang_MaBienThe")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PhieuGiamGiaConfiguration : IEntityTypeConfiguration<PhieuGiamGia>
{
    public void Configure(EntityTypeBuilder<PhieuGiamGia> e)
    {
        e.ToTable("PhieuGiamGia", "dbo");
        e.HasKey(x => x.MaPhieu).HasName("PK_PhieuGiamGia");

        e.Property(x => x.MaCode).HasMaxLength(30).IsUnicode(false);
        e.Property(x => x.KieuGiam).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.GiaTriGiam).HasPrecision(18, 2);
        e.Property(x => x.GiamToiDa).HasPrecision(18, 2);
        e.Property(x => x.DonToiThieu).HasPrecision(18, 2);

        e.HasIndex(x => x.MaCode).IsUnique().HasDatabaseName("UQ_PhieuGiamGia_MaCode");
    }
}

public class KhuyenMaiConfiguration : IEntityTypeConfiguration<KhuyenMai>
{
    public void Configure(EntityTypeBuilder<KhuyenMai> e)
    {
        e.ToTable("KhuyenMai", "dbo");
        e.HasKey(x => x.MaKhuyenMai).HasName("PK_KhuyenMai");

        e.Property(x => x.Ten).HasMaxLength(200);
        e.Property(x => x.PhanTramGiam).HasPrecision(5, 2);

        e.HasOne(x => x.SanPham).WithMany(x => x.KhuyenMais)
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_KhuyenMai_MaSanPham")
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.DanhMuc).WithMany(x => x.KhuyenMais)
            .HasForeignKey(x => x.MaDanhMuc)
            .HasConstraintName("FK_KhuyenMai_MaDanhMuc")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class DonHangConfiguration : IEntityTypeConfiguration<DonHang>
{
    public void Configure(EntityTypeBuilder<DonHang> e)
    {
        e.ToTable("DonHang", "dbo");
        e.HasKey(x => x.MaDonHang).HasName("PK_DonHang");

        e.Property(x => x.TenNguoiNhan).HasMaxLength(150);
        e.Property(x => x.SoDienThoai).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.DiaChiGiaoHang).HasMaxLength(300);
        e.Property(x => x.PhuongThucGiaoHang).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.PhuongThucThanhToan).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.TienHang).HasPrecision(18, 2);
        e.Property(x => x.TienGiam).HasPrecision(18, 2);
        e.Property(x => x.PhiVanChuyen).HasPrecision(18, 2);
        e.Property(x => x.TongTien).HasPrecision(18, 2);
        e.Property(x => x.TrangThai).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.NgayDat).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaNguoiDung, x.NgayDat })
            .IsDescending(false, true)
            .HasDatabaseName("IX_DonHang_MaNguoiDung_NgayDat");
        e.HasIndex(x => x.TrangThai).HasDatabaseName("IX_DonHang_TrangThai");

        e.HasOne(x => x.NguoiDung).WithMany(x => x.DonHangs)
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_DonHang_MaNguoiDung")
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.PhieuGiamGia).WithMany(x => x.DonHangs)
            .HasForeignKey(x => x.MaPhieu)
            .HasConstraintName("FK_DonHang_MaPhieu")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ChiTietDonHangConfiguration : IEntityTypeConfiguration<ChiTietDonHang>
{
    public void Configure(EntityTypeBuilder<ChiTietDonHang> e)
    {
        e.ToTable("ChiTietDonHang", "dbo");
        e.HasKey(x => x.MaChiTietDH).HasName("PK_ChiTietDonHang");

        e.Property(x => x.DonGia).HasPrecision(18, 2);
        e.Property(x => x.ThanhTien).HasPrecision(18, 2);

        e.HasIndex(x => x.MaDonHang).HasDatabaseName("IX_ChiTietDonHang_MaDonHang");

        e.HasOne(x => x.DonHang).WithMany(x => x.ChiTietDonHangs)
            .HasForeignKey(x => x.MaDonHang)
            .HasConstraintName("FK_ChiTietDonHang_MaDonHang")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.BienThe).WithMany()
            .HasForeignKey(x => x.MaBienThe)
            .HasConstraintName("FK_ChiTietDonHang_MaBienThe")
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.LoHang).WithMany()
            .HasForeignKey(x => x.MaLo)
            .HasConstraintName("FK_ChiTietDonHang_MaLo")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
