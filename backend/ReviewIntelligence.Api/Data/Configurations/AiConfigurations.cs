using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data.Configurations;

// Nhóm: Phân tích AI, tác tử & thực nghiệm – KetQuaKhiaCanh, TomTatKhiaCanh, TinNhan,
// NhatKyTacTu, MauKiemThu, KetQuaThucNghiem

public class KetQuaKhiaCanhConfiguration : IEntityTypeConfiguration<KetQuaKhiaCanh>
{
    public void Configure(EntityTypeBuilder<KetQuaKhiaCanh> e)
    {
        e.ToTable("KetQuaKhiaCanh", "dbo");
        e.HasKey(x => x.MaKetQua).HasName("PK_KetQuaKhiaCanh");

        e.Property(x => x.CamXuc).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.CumTuDanChung).HasMaxLength(300);
        e.Property(x => x.MoHinh).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaDanhGia, x.MaKhiaCanh })
            .IsUnique()
            .HasDatabaseName("UQ_KetQuaKhiaCanh_MaDanhGia_MaKhiaCanh");
        e.HasIndex(x => new { x.MaKhiaCanh, x.CamXuc }).HasDatabaseName("IX_KetQuaKhiaCanh_MaKhiaCanh_CamXuc");

        e.HasOne(x => x.DanhGia).WithMany(x => x.KetQuaKhiaCanhs)
            .HasForeignKey(x => x.MaDanhGia)
            .HasConstraintName("FK_KetQuaKhiaCanh_MaDanhGia")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.KhiaCanh).WithMany(x => x.KetQuaKhiaCanhs)
            .HasForeignKey(x => x.MaKhiaCanh)
            .HasConstraintName("FK_KetQuaKhiaCanh_MaKhiaCanh")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TomTatKhiaCanhConfiguration : IEntityTypeConfiguration<TomTatKhiaCanh>
{
    public void Configure(EntityTypeBuilder<TomTatKhiaCanh> e)
    {
        e.ToTable("TomTatKhiaCanh", "dbo");
        e.HasKey(x => x.MaTomTat).HasName("PK_TomTatKhiaCanh");

        e.Property(x => x.UuDiem).HasMaxLength(1000);
        e.Property(x => x.NhuocDiem).HasMaxLength(1000);
        e.Property(x => x.VanDeLapLai).HasMaxLength(500);
        e.Property(x => x.NgayCapNhat).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaSanPham, x.MaKhiaCanh })
            .IsUnique()
            .HasDatabaseName("UQ_TomTatKhiaCanh_MaSanPham_MaKhiaCanh");

        e.HasOne(x => x.SanPham).WithMany(x => x.TomTatKhiaCanhs)
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_TomTatKhiaCanh_MaSanPham")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.KhiaCanh).WithMany(x => x.TomTatKhiaCanhs)
            .HasForeignKey(x => x.MaKhiaCanh)
            .HasConstraintName("FK_TomTatKhiaCanh_MaKhiaCanh")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class TinNhanConfiguration : IEntityTypeConfiguration<TinNhan>
{
    public void Configure(EntityTypeBuilder<TinNhan> e)
    {
        e.ToTable("TinNhan", "dbo");
        e.HasKey(x => x.MaTinNhan).HasName("PK_TinNhan");

        e.Property(x => x.NguoiGui).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaPhien, x.NgayTao }).HasDatabaseName("IX_TinNhan_MaPhien_NgayTao");

        e.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_TinNhan_MaNguoiDung")
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.SanPham).WithMany()
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_TinNhan_MaSanPham")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class NhatKyTacTuConfiguration : IEntityTypeConfiguration<NhatKyTacTu>
{
    public void Configure(EntityTypeBuilder<NhatKyTacTu> e)
    {
        e.ToTable("NhatKyTacTu", "dbo");
        e.HasKey(x => x.MaBuoc).HasName("PK_NhatKyTacTu");

        e.Property(x => x.LoaiBuoc).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.TenCongCu).HasMaxLength(50).IsUnicode(false);

        e.HasIndex(x => new { x.MaTinNhan, x.BuocSo })
            .IsUnique()
            .HasDatabaseName("UQ_NhatKyTacTu_MaTinNhan_BuocSo");

        e.HasOne(x => x.TinNhan).WithMany(x => x.NhatKyTacTus)
            .HasForeignKey(x => x.MaTinNhan)
            .HasConstraintName("FK_NhatKyTacTu_MaTinNhan")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class MauKiemThuConfiguration : IEntityTypeConfiguration<MauKiemThu>
{
    public void Configure(EntityTypeBuilder<MauKiemThu> e)
    {
        e.ToTable("MauKiemThu", "dbo");
        e.HasKey(x => x.MaMau).HasName("PK_MauKiemThu");

        e.Property(x => x.NoiDung).HasMaxLength(2000);
        e.Property(x => x.CamXucChuan).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.TapDuLieu).HasMaxLength(10).IsUnicode(false);

        e.HasOne(x => x.DanhGia).WithMany()
            .HasForeignKey(x => x.MaDanhGia)
            .HasConstraintName("FK_MauKiemThu_MaDanhGia")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class KetQuaThucNghiemConfiguration : IEntityTypeConfiguration<KetQuaThucNghiem>
{
    public void Configure(EntityTypeBuilder<KetQuaThucNghiem> e)
    {
        e.ToTable("KetQuaThucNghiem", "dbo");
        e.HasKey(x => x.MaKetQua).HasName("PK_KetQuaThucNghiem");

        e.Property(x => x.MoHinh).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.CamXucDuDoan).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.NhomLoi).HasMaxLength(100);
        e.Property(x => x.NgayChay).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaMau, x.MoHinh })
            .IsUnique()
            .HasDatabaseName("UQ_KetQuaThucNghiem_MaMau_MoHinh");

        e.HasOne(x => x.MauKiemThu).WithMany(x => x.KetQuaThucNghiems)
            .HasForeignKey(x => x.MaMau)
            .HasConstraintName("FK_KetQuaThucNghiem_MaMau")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
