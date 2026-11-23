using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.Ai;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.Models.Entities;
using ReviewIntelligence.Api.Services.Ai;

namespace ReviewIntelligence.Api.Services;

public interface IAiService
{
    Task<KetQua<HoiDapResponse>> HoiDapAsync(int? maNguoiDung, HoiDapRequest req, CancellationToken ct);
    Task<KetQua<PhienHoiDapDto>> LayPhienAsync(Guid maPhien, int? maNguoiDung, CancellationToken ct);
    Task<KetQuaPhanTrang<TraLoiTomTatDto>> DanhSachTraLoiAsync(TraceQuery q, CancellationToken ct);
    Task<KetQua<TraceChiTietDto>> ChiTietTraceAsync(long maTinNhan, CancellationToken ct);
    Task<BangDieuKhienDto> BangDieuKhienAsync(int? maSanPham, int soNgay, CancellationToken ct);
    Task<ChiSoThucNghiemDto> ChiSoThucNghiemAsync(CancellationToken ct);
    Task<KetQuaPhanTrang<KetQuaMauDto>> KetQuaThucNghiemAsync(KetQuaThucNghiemQuery q, CancellationToken ct);
}

public class AiService(AppDbContext db, IAiServiceClient ai) : IAiService
{
    private static readonly JsonSerializerOptions SnakeCase = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    // ------------------------------------------------------------------ Hỏi đáp

    public async Task<KetQua<HoiDapResponse>> HoiDapAsync(int? maNguoiDung, HoiDapRequest req, CancellationToken ct)
    {
        if (req.MaSanPham is int maSp && !await db.SanPham.AnyAsync(x => x.MaSanPham == maSp, ct))
            return KetQua<HoiDapResponse>.Loi(StatusCodes.Status404NotFound, "Sản phẩm không tồn tại.");

        if (req.MaPhien is Guid phien && !await LaChuPhienAsync(phien, maNguoiDung, ct))
            return KetQua<HoiDapResponse>.Loi(StatusCodes.Status403Forbidden, "Phiên hội thoại không thuộc về bạn.");

        var kq = await ai.GuiAsync(HttpMethod.Post, "hoi-dap", new
        {
            cau_hoi = req.CauHoi.Trim(),
            ma_san_pham = req.MaSanPham,
            ma_nguoi_dung = maNguoiDung,
            ma_phien = req.MaPhien?.ToString()
        }, ct);

        if (!kq.OK || kq.DuLieu is null)
            return KetQua<HoiDapResponse>.Loi(kq.MaHttp >= 500 ? StatusCodes.Status503ServiceUnavailable : kq.MaHttp,
                kq.Loi ?? "Trợ lý AI tạm thời không phản hồi.");

        var res = kq.DuLieu.Value.Deserialize<HoiDapResponse>(SnakeCase)!;
        return KetQua<HoiDapResponse>.ThanhCong(res);
    }

    /// <summary>Phiên ẩn danh (mọi tin nhắn MaNguoiDung NULL) ai có mã phiên đều xem được; phiên gắn tài khoản chỉ chủ phiên.</summary>
    private async Task<bool> LaChuPhienAsync(Guid maPhien, int? maNguoiDung, CancellationToken ct)
    {
        var chu = await db.TinNhan.AsNoTracking()
            .Where(t => t.MaPhien == maPhien && t.MaNguoiDung != null)
            .Select(t => t.MaNguoiDung)
            .Distinct()
            .ToListAsync(ct);
        return chu.Count == 0 || (maNguoiDung is not null && chu.All(c => c == maNguoiDung));
    }

    public async Task<KetQua<PhienHoiDapDto>> LayPhienAsync(Guid maPhien, int? maNguoiDung, CancellationToken ct)
    {
        var tin = await db.TinNhan.AsNoTracking()
            .Where(t => t.MaPhien == maPhien)
            .OrderBy(t => t.NgayTao).ThenBy(t => t.MaTinNhan)
            .Select(t => new { t.MaTinNhan, t.NguoiGui, t.NoiDung, t.DanChung, t.DoTreMs, t.NgayTao, t.MaSanPham })
            .ToListAsync(ct);

        if (tin.Count == 0 || !await LaChuPhienAsync(maPhien, maNguoiDung, ct))
            return KetQua<PhienHoiDapDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy phiên hội thoại.");

        return KetQua<PhienHoiDapDto>.ThanhCong(new PhienHoiDapDto(
            maPhien, tin[0].MaSanPham,
            tin.Select(t => new TinNhanDto(t.MaTinNhan, t.NguoiGui, t.NoiDung, DocJson(t.DanChung), t.DoTreMs, t.NgayTao)).ToList()));
    }

    // ------------------------------------------------------------------ Trace tác tử

    private IQueryable<TraLoiTomTatDto> TruyVanTraLoi(IQueryable<TinNhan> q) =>
        q.Where(t => t.NguoiGui == NguoiGui.TroLy)
            .Select(t => new TraLoiTomTatDto(
                t.MaTinNhan,
                t.MaPhien,
                db.TinNhan
                    .Where(h => h.MaPhien == t.MaPhien && h.NguoiGui == NguoiGui.NguoiDung && h.MaTinNhan < t.MaTinNhan)
                    .OrderByDescending(h => h.MaTinNhan)
                    .Select(h => h.NoiDung)
                    .FirstOrDefault(),
                t.NoiDung,
                t.MaSanPham,
                t.SanPham != null ? t.SanPham.Ten : null,
                t.NguoiDung != null ? t.NguoiDung.HoTen : null,
                t.NhatKyTacTus.Count(),
                t.NhatKyTacTus.Any(b => b.LoaiBuoc == LoaiBuocTacTu.Loi),
                0,
                t.DoTreMs,
                t.NgayTao));

    public async Task<KetQuaPhanTrang<TraLoiTomTatDto>> DanhSachTraLoiAsync(TraceQuery q, CancellationToken ct)
    {
        var goc = db.TinNhan.AsNoTracking().Where(t => t.NguoiGui == NguoiGui.TroLy);
        if (q.MaSanPham is int maSp) goc = goc.Where(t => t.MaSanPham == maSp);

        var tongSo = await goc.CountAsync(ct);
        var trang = await TruyVanTraLoi(goc.OrderByDescending(t => t.MaTinNhan).Skip(q.BoQua).Take(q.KichThuoc))
            .ToListAsync(ct);

        // Đếm dẫn chứng từ cột JSON DanChung (SQL Server không có kiểu mảng – đọc ở tầng ứng dụng).
        var ma = trang.Select(t => t.MaTinNhan).ToList();
        var danChung = await db.TinNhan.AsNoTracking().Where(t => ma.Contains(t.MaTinNhan))
            .Select(t => new { t.MaTinNhan, t.DanChung }).ToDictionaryAsync(x => x.MaTinNhan, x => x.DanChung, ct);

        return new KetQuaPhanTrang<TraLoiTomTatDto>(
            trang.Select(t => t with { SoDanChung = DemMang(danChung.GetValueOrDefault(t.MaTinNhan)) }).ToList(),
            tongSo, q.Trang, q.KichThuoc);
    }

    public async Task<KetQua<TraceChiTietDto>> ChiTietTraceAsync(long maTinNhan, CancellationToken ct)
    {
        var tom = await TruyVanTraLoi(db.TinNhan.AsNoTracking().Where(t => t.MaTinNhan == maTinNhan)).FirstOrDefaultAsync(ct);
        if (tom is null)
            return KetQua<TraceChiTietDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy câu trả lời của trợ lý.");

        var danChung = await db.TinNhan.AsNoTracking().Where(t => t.MaTinNhan == maTinNhan).Select(t => t.DanChung).FirstAsync(ct);
        var buoc = await db.NhatKyTacTu.AsNoTracking()
            .Where(b => b.MaTinNhan == maTinNhan)
            .OrderBy(b => b.BuocSo)
            .Select(b => new { b.BuocSo, b.LoaiBuoc, b.TenCongCu, b.DauVao, b.DauRa, b.DoTreMs })
            .ToListAsync(ct);

        return KetQua<TraceChiTietDto>.ThanhCong(new TraceChiTietDto(
            tom with { SoDanChung = DemMang(danChung) },
            DocJson(danChung),
            buoc.Select(b => new BuocTacTuDto(b.BuocSo, b.LoaiBuoc, b.TenCongCu, DocJson(b.DauVao), DocJson(b.DauRa), b.DoTreMs)).ToList()));
    }

    // ------------------------------------------------------------------ Bảng điều khiển

    public async Task<BangDieuKhienDto> BangDieuKhienAsync(int? maSanPham, int soNgay, CancellationToken ct)
    {
        var khiaCanh = db.VwThongKeKhiaCanh.AsNoTracking();
        if (maSanPham is int maSp) khiaCanh = khiaCanh.Where(x => x.MaSanPham == maSp);
        // Sắp xếp theo khóa nhóm TRƯỚC khi chiếu sang DTO (EF không dịch OrderBy trên thuộc tính record).
        var tkKhiaCanh = await khiaCanh
            .GroupBy(x => new { x.MaKhiaCanh, x.TenKhiaCanh })
            .OrderBy(g => g.Key.MaKhiaCanh)
            .Select(g => new ThongKeKhiaCanhDto(g.Key.MaKhiaCanh, g.Key.TenKhiaCanh,
                g.Sum(x => x.SoTichCuc), g.Sum(x => x.SoTrungTinh), g.Sum(x => x.SoTieuCuc)))
            .ToListAsync(ct);

        var tuNgay = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-(soNgay - 1));
        var xuHuong = db.VwXuHuongCamXuc.AsNoTracking().Where(x => x.Ngay >= tuNgay);
        if (maSanPham is int maSp2) xuHuong = xuHuong.Where(x => x.MaSanPham == maSp2);
        var tkXuHuong = await xuHuong
            .GroupBy(x => x.Ngay)
            .OrderBy(g => g.Key)
            .Select(g => new XuHuongCamXucDto(g.Key, g.Sum(x => x.SoTichCuc), g.Sum(x => x.SoTrungTinh), g.Sum(x => x.SoTieuCuc)))
            .ToListAsync(ct);

        return new BangDieuKhienDto(maSanPham, soNgay, tkKhiaCanh, tkXuHuong);
    }

    // ------------------------------------------------------------------ Thực nghiệm

    public async Task<ChiSoThucNghiemDto> ChiSoThucNghiemAsync(CancellationToken ct)
    {
        var boMau = await db.MauKiemThu.AsNoTracking()
            .GroupBy(m => new { m.TapDuLieu, m.CamXucChuan })
            .Select(g => new { g.Key.TapDuLieu, g.Key.CamXucChuan, SoMau = g.Count() })
            .ToListAsync(ct);
        var thongTinBoMau = new ThongTinBoMauDto(
            boMau.Where(x => x.TapDuLieu == TapDuLieu.KiemThu).Sum(x => x.SoMau),
            boMau.Where(x => x.TapDuLieu == TapDuLieu.ViDuMau).Sum(x => x.SoMau),
            boMau.Where(x => x.TapDuLieu == TapDuLieu.KiemThu).ToDictionary(x => x.CamXucChuan, x => x.SoMau));

        var view = await db.VwChiSoThucNghiem.AsNoTracking().ToListAsync(ct);

        // Dữ liệu khía cạnh cho chỉ số bổ sung (chỉ tập KiemThu, giống view).
        var dong = await db.KetQuaThucNghiem.AsNoTracking()
            .Where(k => k.MauKiemThu.TapDuLieu == TapDuLieu.KiemThu)
            .Select(k => new { k.MoHinh, k.KhiaCanhDuDoan, k.MauKiemThu.KhiaCanhChuan, k.NgayChay })
            .ToListAsync(ct);

        var moHinh = view
            .OrderBy(v => Array.IndexOf(MoHinhAbsa.TatCa, v.MoHinh))
            .Select(v =>
            {
                var cuaMoHinh = dong.Where(d => d.MoHinh == v.MoHinh).ToList();
                return new ChiSoMoHinhDto(
                    v.MoHinh, v.SoMau, v.DoChinhXac, v.DoChuanXacMacro, v.DoPhuMacro, v.DiemF1Macro, v.DoTreTrungBinhMs,
                    TinhChiSoKhiaCanh(cuaMoHinh.Select(d => (DocTuDien(d.KhiaCanhChuan), DocTuDien(d.KhiaCanhDuDoan)))),
                    cuaMoHinh.Count > 0 ? cuaMoHinh.Max(d => d.NgayChay) : null);
            })
            .ToList();

        return new ChiSoThucNghiemDto(thongTinBoMau, moHinh);
    }

    /// <summary>Micro precision / recall / F1 cho việc phát hiện khía cạnh + độ chính xác cảm xúc trên các khía cạnh phát hiện đúng.</summary>
    private static ChiSoKhiaCanhDto TinhChiSoKhiaCanh(IEnumerable<(Dictionary<string, string> Chuan, Dictionary<string, string> DuDoan)> cap)
    {
        int tp = 0, fp = 0, fn = 0, dungCamXuc = 0, soCapChuan = 0;
        foreach (var (chuan, duDoan) in cap)
        {
            soCapChuan += chuan.Count;
            foreach (var (k, v) in duDoan)
            {
                if (chuan.TryGetValue(k, out var c)) { tp++; if (c == v) dungCamXuc++; }
                else fp++;
            }
            fn += chuan.Keys.Count(k => !duDoan.ContainsKey(k));
        }
        double p = tp + fp == 0 ? 0 : (double)tp / (tp + fp);
        double r = tp + fn == 0 ? 0 : (double)tp / (tp + fn);
        double f1 = p + r == 0 ? 0 : 2 * p * r / (p + r);
        return new ChiSoKhiaCanhDto(p, r, f1, tp == 0 ? 0 : (double)dungCamXuc / tp, soCapChuan);
    }

    public async Task<KetQuaPhanTrang<KetQuaMauDto>> KetQuaThucNghiemAsync(KetQuaThucNghiemQuery q, CancellationToken ct)
    {
        var query = db.KetQuaThucNghiem.AsNoTracking()
            .Where(k => k.MoHinh == q.MoHinh && k.MauKiemThu.TapDuLieu == TapDuLieu.KiemThu);
        if (q.ChiSai) query = query.Where(k => !k.LaDung);

        var tongSo = await query.CountAsync(ct);
        var trang = await query.OrderBy(k => k.MaMau).Skip(q.BoQua).Take(q.KichThuoc)
            .Select(k => new
            {
                k.MaMau, k.MauKiemThu.NoiDung, k.MauKiemThu.CamXucChuan, k.CamXucDuDoan, k.MauKiemThu.KhiaCanhChuan,
                k.KhiaCanhDuDoan, k.LaDung, k.NhomLoi, k.DoTreMs, k.NgayChay
            })
            .ToListAsync(ct);

        return new KetQuaPhanTrang<KetQuaMauDto>(
            trang.Select(k => new KetQuaMauDto(k.MaMau, k.NoiDung, k.CamXucChuan, k.CamXucDuDoan,
                DocTuDien(k.KhiaCanhChuan), DocTuDien(k.KhiaCanhDuDoan), k.LaDung, k.NhomLoi, k.DoTreMs, k.NgayChay)).ToList(),
            tongSo, q.Trang, q.KichThuoc);
    }

    // ------------------------------------------------------------------ JSON helpers

    private static JsonElement? DocJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonDocument.Parse(json).RootElement.Clone(); }
        catch (JsonException) { return null; }
    }

    private static int DemMang(string? json) =>
        DocJson(json) is { ValueKind: JsonValueKind.Array } a ? a.GetArrayLength() : 0;

    private static Dictionary<string, string> DocTuDien(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? []; }
        catch (JsonException) { return []; }
    }
}
