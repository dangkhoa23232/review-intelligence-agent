using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.TraCuu;

namespace ReviewIntelligence.Api.Services;

/// <summary>Dữ liệu tra cứu dùng cho bộ lọc / form: DanhMuc, ThuongHieu, LoaiDa, KhiaCanh.</summary>
public interface ITraCuuService
{
    Task<IReadOnlyList<DanhMucDto>> DanhMucAsync(CancellationToken ct);
    Task<IReadOnlyList<ThuongHieuDto>> ThuongHieuAsync(CancellationToken ct);
    Task<IReadOnlyList<LoaiDaDto>> LoaiDaAsync(CancellationToken ct);
    Task<IReadOnlyList<KhiaCanhDto>> KhiaCanhAsync(CancellationToken ct);
}

public class TraCuuService(AppDbContext db) : ITraCuuService
{
    public async Task<IReadOnlyList<DanhMucDto>> DanhMucAsync(CancellationToken ct) =>
        await db.DanhMuc.AsNoTracking()
            .OrderBy(x => x.MaDanhMucCha.HasValue).ThenBy(x => x.MaDanhMucCha).ThenBy(x => x.Ten)
            .Select(x => new DanhMucDto(x.MaDanhMuc, x.MaDanhMucCha, x.Ten, x.DuongDan))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<ThuongHieuDto>> ThuongHieuAsync(CancellationToken ct) =>
        await db.ThuongHieu.AsNoTracking()
            .OrderBy(x => x.Ten)
            .Select(x => new ThuongHieuDto(x.MaThuongHieu, x.Ten, x.QuocGia))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<LoaiDaDto>> LoaiDaAsync(CancellationToken ct) =>
        await db.LoaiDa.AsNoTracking()
            .OrderBy(x => x.MaLoaiDa)
            .Select(x => new LoaiDaDto(x.MaLoaiDa, x.Ten))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<KhiaCanhDto>> KhiaCanhAsync(CancellationToken ct) =>
        await db.KhiaCanh.AsNoTracking()
            .OrderBy(x => x.MaKhiaCanh)
            .Select(x => new KhiaCanhDto(x.MaKhiaCanh, x.Ten, x.MoTa))
            .ToListAsync(ct);
}
