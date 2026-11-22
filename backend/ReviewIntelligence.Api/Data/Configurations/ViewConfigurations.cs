using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReviewIntelligence.Api.Models.Views;

namespace ReviewIntelligence.Api.Data.Configurations;

// Các VIEW chỉ đọc – HasNoKey + ToView. Không SaveChanges trên các entity này.

public class DoanhThuTheoNgayConfiguration : IEntityTypeConfiguration<DoanhThuTheoNgay>
{
    public void Configure(EntityTypeBuilder<DoanhThuTheoNgay> e)
    {
        e.HasNoKey().ToView("vw_DoanhThuTheoNgay", "dbo");
        e.Property(x => x.Ngay).HasColumnType("date");
        e.Property(x => x.DoanhThu).HasPrecision(38, 2);
    }
}

public class SanPhamBanChayConfiguration : IEntityTypeConfiguration<SanPhamBanChay>
{
    public void Configure(EntityTypeBuilder<SanPhamBanChay> e)
    {
        e.HasNoKey().ToView("vw_SanPhamBanChay", "dbo");
        e.Property(x => x.DoanhThu).HasPrecision(38, 2);
    }
}

public class TonKhoThapConfiguration : IEntityTypeConfiguration<TonKhoThap>
{
    public void Configure(EntityTypeBuilder<TonKhoThap> e)
    {
        e.HasNoKey().ToView("vw_TonKhoThap", "dbo");
        e.Property(x => x.MaSKU).IsUnicode(false);
        e.Property(x => x.HanSuDungGanNhat).HasColumnType("date");
    }
}

public class ThongKeKhiaCanhConfiguration : IEntityTypeConfiguration<ThongKeKhiaCanh>
{
    public void Configure(EntityTypeBuilder<ThongKeKhiaCanh> e)
    {
        e.HasNoKey().ToView("vw_ThongKeKhiaCanh", "dbo");
    }
}

public class XuHuongCamXucConfiguration : IEntityTypeConfiguration<XuHuongCamXuc>
{
    public void Configure(EntityTypeBuilder<XuHuongCamXuc> e)
    {
        e.HasNoKey().ToView("vw_XuHuongCamXuc", "dbo");
        e.Property(x => x.Ngay).HasColumnType("date");
    }
}

public class ChiSoThucNghiemConfiguration : IEntityTypeConfiguration<ChiSoThucNghiem>
{
    public void Configure(EntityTypeBuilder<ChiSoThucNghiem> e)
    {
        e.HasNoKey().ToView("vw_ChiSoThucNghiem", "dbo");
        e.Property(x => x.MoHinh).IsUnicode(false);
    }
}
