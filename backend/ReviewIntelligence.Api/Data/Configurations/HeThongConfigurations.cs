using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data.Configurations;

// Nhóm: Thông báo & nhật ký – ThongBao, NhatKyHeThong

public class ThongBaoConfiguration : IEntityTypeConfiguration<ThongBao>
{
    public void Configure(EntityTypeBuilder<ThongBao> e)
    {
        e.ToTable("ThongBao", "dbo");
        e.HasKey(x => x.MaThongBao).HasName("PK_ThongBao");

        e.Property(x => x.NoiDung).HasMaxLength(500);
        e.Property(x => x.LienKet).HasMaxLength(300);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaNguoiDung, x.DaDoc }).HasDatabaseName("IX_ThongBao_MaNguoiDung_DaDoc");

        e.HasOne(x => x.NguoiDung).WithMany(x => x.ThongBaos)
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_ThongBao_MaNguoiDung")
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class NhatKyHeThongConfiguration : IEntityTypeConfiguration<NhatKyHeThong>
{
    public void Configure(EntityTypeBuilder<NhatKyHeThong> e)
    {
        e.ToTable("NhatKyHeThong", "dbo");
        e.HasKey(x => x.MaNhatKy).HasName("PK_NhatKyHeThong");

        e.Property(x => x.HanhDong).HasMaxLength(50).IsUnicode(false);
        e.Property(x => x.DoiTuong).HasMaxLength(100).IsUnicode(false);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => x.NgayTao).IsDescending().HasDatabaseName("IX_NhatKyHeThong_NgayTao");

        e.HasOne(x => x.NguoiDung).WithMany()
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_NhatKyHeThong_MaNguoiDung")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
