using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data;

/// <summary>
/// DbContext ánh xạ tới ReviewIntelligenceDB (database-first).
/// Schema gốc: database/ReviewIntelligence_SQLServer.sql – KHÔNG dùng EF migrations để tạo/sửa bảng.
/// Tên bảng/cột phải khớp chính xác schema.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LoaiDa> LoaiDa => Set<LoaiDa>();
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();
    public DbSet<KhiaCanh> KhiaCanh => Set<KhiaCanh>();

    // TODO: bổ sung 25 entity còn lại (SanPham, DanhGia, DonHang, ...).
    // Lưu ý: bảng DanhGia có trigger trg_DanhGia_CapNhatDiem → phải khai báo
    //   e.ToTable("DanhGia", "dbo", t => t.HasTrigger("trg_DanhGia_CapNhatDiem"));
    // nếu không EF Core 8 sẽ lỗi khi SaveChanges (OUTPUT clause + trigger).

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
