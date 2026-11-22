using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data.Configurations;

// Nhóm: Đánh giá – KhiaCanh, DanhGia, DiemTieuChi, BaoCaoDanhGia

public class KhiaCanhConfiguration : IEntityTypeConfiguration<KhiaCanh>
{
    public void Configure(EntityTypeBuilder<KhiaCanh> e)
    {
        e.ToTable("KhiaCanh", "dbo");
        e.HasKey(x => x.MaKhiaCanh).HasName("PK_KhiaCanh");
        e.Property(x => x.Ten).HasMaxLength(50);
        e.Property(x => x.MoTa).HasMaxLength(300);
        e.HasIndex(x => x.Ten).IsUnique().HasDatabaseName("UQ_KhiaCanh_Ten");
    }
}

public class DanhGiaConfiguration : IEntityTypeConfiguration<DanhGia>
{
    /// <summary>Trigger AFTER INSERT/UPDATE/DELETE cập nhật SanPham.DiemTrungBinh, SanPham.SoDanhGia.</summary>
    public const string TriggerCapNhatDiem = "trg_DanhGia_CapNhatDiem";

    public void Configure(EntityTypeBuilder<DanhGia> e)
    {
        // Khai báo trigger để EF Core 8 không dùng mệnh đề OUTPUT (SQL Server cấm OUTPUT
        // không có INTO trên bảng có trigger). Sau SaveChanges, SanPham đang được track sẽ
        // KHÔNG tự cập nhật DiemTrungBinh/SoDanhGia → cần Reload() hoặc truy vấn lại.
        e.ToTable("DanhGia", "dbo", t => t.HasTrigger(TriggerCapNhatDiem));
        e.HasKey(x => x.MaDanhGia).HasName("PK_DanhGia");

        e.Property(x => x.NoiDung).HasMaxLength(2000);
        e.Property(x => x.TrangThai).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.CamXuc).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.DoTinCay).HasPrecision(5, 4);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaSanPham, x.TrangThai }).HasDatabaseName("IX_DanhGia_MaSanPham_TrangThai");
        e.HasIndex(x => x.MaNguoiDung).HasDatabaseName("IX_DanhGia_MaNguoiDung");
        e.HasIndex(x => x.MaChiTietDH)
            .IsUnique()
            .HasFilter("[MaChiTietDH] IS NOT NULL")
            .HasDatabaseName("UX_DanhGia_MaChiTietDH");

        e.HasOne(x => x.SanPham).WithMany(x => x.DanhGias)
            .HasForeignKey(x => x.MaSanPham)
            .HasConstraintName("FK_DanhGia_MaSanPham")
            .OnDelete(DeleteBehavior.NoAction);
        e.HasOne(x => x.NguoiDung).WithMany(x => x.DanhGias)
            .HasForeignKey(x => x.MaNguoiDung)
            .HasConstraintName("FK_DanhGia_MaNguoiDung")
            .OnDelete(DeleteBehavior.NoAction);
        // 1–0..1 nhờ chỉ mục duy nhất có lọc UX_DanhGia_MaChiTietDH.
        e.HasOne(x => x.ChiTietDonHang).WithOne(x => x.DanhGia)
            .HasForeignKey<DanhGia>(x => x.MaChiTietDH)
            .HasConstraintName("FK_DanhGia_MaChiTietDH")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class DiemTieuChiConfiguration : IEntityTypeConfiguration<DiemTieuChi>
{
    public void Configure(EntityTypeBuilder<DiemTieuChi> e)
    {
        e.ToTable("DiemTieuChi", "dbo");
        e.HasKey(x => new { x.MaDanhGia, x.MaKhiaCanh }).HasName("PK_DiemTieuChi");

        e.HasOne(x => x.DanhGia).WithMany(x => x.DiemTieuChis)
            .HasForeignKey(x => x.MaDanhGia)
            .HasConstraintName("FK_DiemTieuChi_MaDanhGia")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.KhiaCanh).WithMany(x => x.DiemTieuChis)
            .HasForeignKey(x => x.MaKhiaCanh)
            .HasConstraintName("FK_DiemTieuChi_MaKhiaCanh")
            .OnDelete(DeleteBehavior.NoAction);
    }
}

public class BaoCaoDanhGiaConfiguration : IEntityTypeConfiguration<BaoCaoDanhGia>
{
    public void Configure(EntityTypeBuilder<BaoCaoDanhGia> e)
    {
        e.ToTable("BaoCaoDanhGia", "dbo");
        e.HasKey(x => x.MaBaoCao).HasName("PK_BaoCaoDanhGia");

        e.Property(x => x.LyDo).HasMaxLength(300);
        e.Property(x => x.TrangThai).HasMaxLength(10).IsUnicode(false);
        e.Property(x => x.NgayTao).HasDefaultValueSql("SYSUTCDATETIME()");

        e.HasIndex(x => new { x.MaDanhGia, x.MaNguoiBaoCao })
            .IsUnique()
            .HasDatabaseName("UQ_BaoCaoDanhGia_MaDanhGia_MaNguoiBaoCao");

        e.HasOne(x => x.DanhGia).WithMany(x => x.BaoCaoDanhGias)
            .HasForeignKey(x => x.MaDanhGia)
            .HasConstraintName("FK_BaoCaoDanhGia_MaDanhGia")
            .OnDelete(DeleteBehavior.Cascade);
        e.HasOne(x => x.NguoiBaoCao).WithMany(x => x.BaoCaoDanhGias)
            .HasForeignKey(x => x.MaNguoiBaoCao)
            .HasConstraintName("FK_BaoCaoDanhGia_MaNguoiBaoCao")
            .OnDelete(DeleteBehavior.NoAction);
    }
}
