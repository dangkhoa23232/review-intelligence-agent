using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data.Configurations;

// Nhóm: Sản phẩm & tồn kho – DanhMuc, ThuongHieu, SanPham, SanPham_LoaiDa, BienThe,
// LoHang, BoChamSoc, ChiTietBoChamSoc, YeuThich

public class DanhMucConfiguration : IEntityTypeConfiguration<DanhMuc>
{
    public void Configure(EntityTypeBuilder<DanhMuc> e)
    {
        e.ToTable("DanhMuc", "dbo");
        e.HasKey(x => x.MaDanhMuc).HasName("PK_DanhMuc");
        e.Property(x => x.Ten).HasMaxLength(150);
        e.Property(x => x.DuongDan).HasMaxLength(160).IsUnicode(false);
        e.HasIndex(x => x.DuongDan).IsUnique().HasDatabaseName("UQ_DanhMuc_DuongDan");

        e.HasOne(x => x.DanhMucCha).WithMany(x => x.DanhMucCons)
            .HasForeignKey(x => x.MaDanhMucCha)
            .HasConstraintName("FK_DanhMuc_MaDanhMucCha")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ThuongHieuConfiguration : IEntityTypeConfiguration<ThuongHieu>
{
    public void Configure(EntityTypeBuilder<ThuongHieu> e)
    {
        e.ToTable("ThuongHieu", "dbo");
        e.HasKey(x => x.MaThuongHieu).HasName("PK_ThuongHieu");
        e.Property(x => x.Ten).HasMaxLength(150);
        e.Property(x => x.QuocGia).HasMaxLength(100);
        e.HasIndex(x => x.Ten).IsUnique().HasDatabaseName("UQ_ThuongHieu_Ten");
    }
}

public class SanPhamConfiguration : IEntityTypeConfiguration<SanPham>
{
    public void Configure(EntityTypeBuilder<SanPham> e)
    {
        e.ToTable("SanPham", "dbo");
        e.HasKey(x => x.MaSanPham).HasName("PK_SanPham");

        e.Property(x => x.Ten).HasMaxLength(255);
        e.Property(x => x.DuongDan).HasMaxLength(280).IsUnicode(false);
        e.Property(x => x.CongDung).HasMaxLength(300);
        e.Property(x => x.HuongDanSuDung).HasMaxLength(1000);
        e.Property(x => x.TrangThai).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        // DiemTrungBinh / SoDanhGia do trigger trg_DanhGia_CapNhatDiem duy trì:
        // INSERT để DB dùng DEFAULT (0), UPDATE không bao giờ ghi đè giá trị trigger đã tính.
        e.Property(x => x.DiemTrungBinh).HasPrecision(3, 2).HasDefaultValue(0m)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
        e.Property(x => x.SoDanhGia).HasDefaultValue(0)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

        e.HasIndex(x => x.DuongDan).IsUnique().HasDatabaseName("UQ_SanPham_DuongDan");
        e.HasIndex(x => x.MaDanhMuc).HasDatabaseName("IX_SanPham_MaDanhMuc");
        e.HasIndex(x => x.MaThuongHieu).HasDatabaseName("IX_SanPham_MaThuongHieu");
        e.HasIndex(x => x.Ten).HasDatabaseName("IX_SanPham_Ten");

        e.HasOne(x => x.DanhMuc).WithMany(x => x.SanPhams)
            .HasForeignKey(x => x.MaDanhMuc)
            .HasConstraintName("FK_SanPham_MaDanhMuc")
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.ThuongHieu).WithMany(x => x.SanPhams)
            .HasForeignKey(x => x.MaThuongHieu)
            .HasConstraintName("FK_SanPham_MaThuongHieu")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class SanPhamLoaiDaConfiguration : IEntityTypeConfiguration<SanPhamLoaiDa>
{
    public void Configure(EntityTypeBuilder<SanPhamLoaiDa> e)
    {
        e.ToTable("SanPham_LoaiDa", "dbo");
        e.HasKey(x => new { x.MaSanPham, x.MaLoaiDa }).HasName("PK_SanPham_LoaiDa");

        e.HasOne(x => x.SanPham).WithMany(x => x.SanPhamLoaiDas)
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_SanPham_LoaiDa_MaSanPham")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.LoaiDa).WithMany(x => x.SanPhamLoaiDas)
            .HasForeignKey(x => x.MaLoaiDa)
            .HasConstraintName("FK_SanPham_LoaiDa_MaLoaiDa")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BienTheConfiguration : IEntityTypeConfiguration<BienThe>
{
    public void Configure(EntityTypeBuilder<BienThe> e)
    {
        e.ToTable("BienThe", "dbo");
        e.HasKey(x => x.MaBienThe).HasName("PK_BienThe");

        e.Property(x => x.MaSKU).HasMaxLength(50).IsUnicode(false);
        e.Property(x => x.DungTich).HasMaxLength(30);
        e.Property(x => x.GiaBan).HasPrecision(18, 2);
        e.Property(x => x.GiaKhuyenMai).HasPrecision(18, 2);

        e.HasIndex(x => x.MaSKU).IsUnique().HasDatabaseName("UQ_BienThe_MaSKU");
        e.HasIndex(x => x.MaSanPham).HasDatabaseName("IX_BienThe_MaSanPham");

        e.HasOne(x => x.SanPham).WithMany(x => x.BienThes)
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_BienThe_MaSanPham")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class LoHangConfiguration : IEntityTypeConfiguration<LoHang>
{
    public void Configure(EntityTypeBuilder<LoHang> e)
    {
        e.ToTable("LoHang", "dbo");
        e.HasKey(x => x.MaLo).HasName("PK_LoHang");

        e.Property(x => x.SoLo).HasMaxLength(50).IsUnicode(false);
        e.Property(x => x.HanSuDung).HasColumnType("date");
        e.Property(x => x.NgayNhap).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaBienThe, x.SoLo }).IsUnique().HasDatabaseName("UQ_LoHang_MaBienThe_SoLo");
        e.HasIndex(x => new { x.MaBienThe, x.HanSuDung }).HasDatabaseName("IX_LoHang_MaBienThe_HanSuDung");

        e.HasOne(x => x.BienThe).WithMany(x => x.LoHangs)
            .HasForeignKey(x => x.MaBienThe)
            .HasConstraintName("FK_LoHang_MaBienThe")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class BoChamSocConfiguration : IEntityTypeConfiguration<BoChamSoc>
{
    public void Configure(EntityTypeBuilder<BoChamSoc> e)
    {
        e.ToTable("BoChamSoc", "dbo");
        e.HasKey(x => x.MaBo).HasName("PK_BoChamSoc");

        e.Property(x => x.Ten).HasMaxLength(200);
        e.Property(x => x.MoTa).HasMaxLength(1000);
        e.Property(x => x.PhanTramGiam).HasPrecision(5, 2);

        e.HasOne(x => x.LoaiDa).WithMany(x => x.BoChamSocs)
            .HasForeignKey(x => x.MaLoaiDa)
            .HasConstraintName("FK_BoChamSoc_MaLoaiDa")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class ChiTietBoChamSocConfiguration : IEntityTypeConfiguration<ChiTietBoChamSoc>
{
    public void Configure(EntityTypeBuilder<ChiTietBoChamSoc> e)
    {
        e.ToTable("ChiTietBoChamSoc", "dbo");
        e.HasKey(x => new { x.MaBo, x.MaBienThe }).HasName("PK_ChiTietBoChamSoc");
        e.Property(x => x.TenBuoc).HasMaxLength(100);

        e.HasOne(x => x.BoChamSoc).WithMany(x => x.ChiTietBoChamSocs)
            .HasForeignKey(x => x.MaBo)
            .HasConstraintName("FK_ChiTietBoChamSoc_MaBo")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.BienThe).WithMany()
            .HasForeignKey(x => x.MaBienThe)
            .HasConstraintName("FK_ChiTietBoChamSoc_MaBienThe")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class YeuThichConfiguration : IEntityTypeConfiguration<YeuThich>
{
    public void Configure(EntityTypeBuilder<YeuThich> e)
    {
        e.ToTable("YeuThich", "dbo");
        e.HasKey(x => new { x.MaNguoiDung, x.MaSanPham }).HasName("PK_YeuThich");
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasOne(x => x.NguoiDung).WithMany(x => x.YeuThichs)
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_YeuThich_MaNguoiDung")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_YeuThich_MaSanPham")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
