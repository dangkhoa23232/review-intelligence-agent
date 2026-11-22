using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data.Configurations;

public class LoaiDaConfiguration : IEntityTypeConfiguration<LoaiDa>
{
    public void Configure(EntityTypeBuilder<LoaiDa> e)
    {
        e.ToTable("LoaiDa", "dbo");
        e.HasKey(x => x.MaLoaiDa).HasName("PK_LoaiDa");
        e.Property(x => x.Ten).HasMaxLength(50).IsRequired();
        e.HasIndex(x => x.Ten).IsUnique().HasDatabaseName("UQ_LoaiDa_Ten");
    }
}

public class NguoiDungConfiguration : IEntityTypeConfiguration<NguoiDung>
{
    public void Configure(EntityTypeBuilder<NguoiDung> e)
    {
        e.ToTable("NguoiDung", "dbo");
        e.HasKey(x => x.MaNguoiDung).HasName("PK_NguoiDung");

        e.Property(x => x.Email).HasMaxLength(255).IsRequired();
        e.Property(x => x.MatKhauBam).HasMaxLength(255).IsUnicode(false).IsRequired();
        e.Property(x => x.HoTen).HasMaxLength(150).IsRequired();
        e.Property(x => x.SoDienThoai).HasMaxLength(20).IsUnicode(false);
        e.Property(x => x.DiaChi).HasMaxLength(300);
        e.Property(x => x.VaiTro).HasMaxLength(10).IsUnicode(false).HasDefaultValue(VaiTroNguoiDung.KhachHang);
        e.Property(x => x.MaDatLaiMatKhau).HasMaxLength(255).IsUnicode(false);
        e.Property(x => x.HanMaDatLai).HasColumnType("datetime2(0)");
        e.Property(x => x.DangHoatDong).HasDefaultValue(true);
        e.Property(x => x.NgayTao).HasColumnType("datetime2(0)").HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_NguoiDung_Email");

        e.HasOne(x => x.LoaiDa).WithMany(x => x.NguoiDungs)
            .HasForeignKey(x => x.MaLoaiDa)
            .HasConstraintName("FK_NguoiDung_MaLoaiDa");
    }
}

public class KhiaCanhConfiguration : IEntityTypeConfiguration<KhiaCanh>
{
    public void Configure(EntityTypeBuilder<KhiaCanh> e)
    {
        e.ToTable("KhiaCanh", "dbo");
        e.HasKey(x => x.MaKhiaCanh).HasName("PK_KhiaCanh");
        e.Property(x => x.Ten).HasMaxLength(50).IsRequired();
        e.Property(x => x.MoTa).HasMaxLength(300);
        e.HasIndex(x => x.Ten).IsUnique().HasDatabaseName("UQ_KhiaCanh_Ten");
    }
}
