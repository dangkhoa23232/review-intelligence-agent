using Microsoft.Extensions.Options;
using ReviewIntelligence.Api.DTOs.SanPham;
using ReviewIntelligence.Api.Options;

namespace ReviewIntelligence.Api.Services;

public interface IAnhSanPhamService
{
    Task<KetQua<AnhDaTaiLenDto>> LuuAsync(IFormFile? tep, CancellationToken ct);
}

/// <summary>
/// Lưu ảnh sản phẩm vào thư mục public của frontend. Chỉ nhận .jpg/.jpeg/.png/.webp và kiểm tra
/// chữ ký nhị phân (magic bytes) – không tin phần mở rộng hay Content-Type do client gửi.
/// Tên tệp do server sinh (slug + mã ngẫu nhiên) nên không thể ghi đè hay thoát khỏi thư mục lưu.
/// </summary>
public class AnhSanPhamService(IOptions<AnhSanPhamOptions> options, IWebHostEnvironment env, ILogger<AnhSanPhamService> logger)
    : IAnhSanPhamService
{
    private static readonly Dictionary<string, string> DuoiHopLe = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = ".jpg", [".jpeg"] = ".jpg", [".png"] = ".png", [".webp"] = ".webp"
    };

    public async Task<KetQua<AnhDaTaiLenDto>> LuuAsync(IFormFile? tep, CancellationToken ct)
    {
        var cfg = options.Value;
        var toiDa = cfg.DungLuongToiDaMb * 1024L * 1024L;

        if (tep is null || tep.Length == 0)
            return Loi("Vui lòng chọn một tệp ảnh (trường 'tep').");
        if (tep.Length > toiDa)
            return Loi($"Ảnh vượt quá {cfg.DungLuongToiDaMb} MB.");
        if (!DuoiHopLe.TryGetValue(Path.GetExtension(tep.FileName), out var duoi))
            return Loi("Chỉ chấp nhận ảnh .jpg, .png hoặc .webp.");

        // Đọc vào bộ nhớ (≤ DungLuongToiDaMb) để kiểm tra chữ ký trước khi ghi đĩa.
        await using var ms = new MemoryStream((int)tep.Length);
        await tep.CopyToAsync(ms, ct);

        var loaiThuc = NhanDang(ms.GetBuffer().AsSpan(0, (int)ms.Length));
        if (loaiThuc is null)
            return Loi("Nội dung tệp không phải ảnh JPG, PNG hoặc WebP hợp lệ.");
        if (loaiThuc != duoi)
            return Loi($"Phần mở rộng '{Path.GetExtension(tep.FileName)}' không khớp nội dung ảnh ({loaiThuc}).");

        var thuMuc = Path.GetFullPath(Path.Combine(env.ContentRootPath, cfg.ThuMucLuu));
        Directory.CreateDirectory(thuMuc);

        var goc = AdminSanPhamService.TaoDuongDan(Path.GetFileNameWithoutExtension(tep.FileName));
        if (goc.Length == 0) goc = "anh";
        if (goc.Length > 60) goc = goc[..60].TrimEnd('-');
        var tenTep = $"{goc}-{Guid.NewGuid().ToString("N")[..8]}{duoi}";
        var duongDanVatLy = Path.Combine(thuMuc, tenTep);

        await using (var fs = new FileStream(duongDanVatLy, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            ms.Position = 0;
            await ms.CopyToAsync(fs, ct);
        }

        var congKhai = $"{cfg.DuongDanCongKhai.TrimEnd('/')}/{tenTep}";
        logger.LogInformation("Đã lưu ảnh sản phẩm {DuongDan} ({KichThuoc} byte).", congKhai, ms.Length);
        return KetQua<AnhDaTaiLenDto>.ThanhCong(new AnhDaTaiLenDto(congKhai, tenTep, ms.Length));
    }

    private static KetQua<AnhDaTaiLenDto> Loi(string thongBao) =>
        KetQua<AnhDaTaiLenDto>.Loi(StatusCodes.Status400BadRequest, thongBao);

    /// <summary>Trả phần mở rộng chuẩn theo magic bytes, hoặc null nếu không nhận ra.</summary>
    private static string? NhanDang(ReadOnlySpan<byte> b)
    {
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF) return ".jpg";
        if (b.Length >= 8 && b[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return ".png";
        if (b.Length >= 12 && b[..4].SequenceEqual("RIFF"u8) && b[8..12].SequenceEqual("WEBP"u8)) return ".webp";
        return null;
    }
}
