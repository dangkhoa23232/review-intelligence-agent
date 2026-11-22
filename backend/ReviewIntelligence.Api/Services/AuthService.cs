using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.Auth;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Services;

public interface IAuthService
{
    Task<KetQua<TokenResponse>> DangNhapAsync(DangNhapRequest req, CancellationToken ct);
    Task<KetQua<NguoiDungDto>> DangKyAsync(DangKyRequest req, CancellationToken ct);
    Task<KetQua<HoSoDto>> LayHoSoAsync(int maNguoiDung, CancellationToken ct);
}

public class AuthService(AppDbContext db, ITokenService tokenService) : IAuthService
{
    public async Task<KetQua<HoSoDto>> LayHoSoAsync(int maNguoiDung, CancellationToken ct)
    {
        var hoSo = await db.NguoiDung.AsNoTracking()
            .Where(x => x.MaNguoiDung == maNguoiDung && x.DangHoatDong)
            .Select(x => new HoSoDto(x.MaNguoiDung, x.Email, x.HoTen, x.SoDienThoai, x.DiaChi, x.MaLoaiDa, x.VaiTro))
            .FirstOrDefaultAsync(ct);

        return hoSo is null
            ? KetQua<HoSoDto>.Loi(StatusCodes.Status404NotFound, "Không tìm thấy tài khoản.")
            : KetQua<HoSoDto>.ThanhCong(hoSo);
    }

    public async Task<KetQua<TokenResponse>> DangNhapAsync(DangNhapRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();
        var nd = await db.NguoiDung.AsNoTracking().FirstOrDefaultAsync(x => x.Email == email, ct);

        // Không tiết lộ email có tồn tại hay không.
        if (nd is null || !XacThucMatKhau(req.MatKhau, nd.MatKhauBam))
            return KetQua<TokenResponse>.Loi(StatusCodes.Status401Unauthorized, "Email hoặc mật khẩu không đúng.");

        if (!nd.DangHoatDong)
            return KetQua<TokenResponse>.Loi(StatusCodes.Status403Forbidden, "Tài khoản đã bị khóa.");

        var (token, hetHan) = tokenService.TaoAccessToken(nd);
        return KetQua<TokenResponse>.ThanhCong(new TokenResponse(token, hetHan, MapDto(nd)));
    }

    public async Task<KetQua<NguoiDungDto>> DangKyAsync(DangKyRequest req, CancellationToken ct)
    {
        var email = req.Email.Trim().ToLowerInvariant();

        if (await db.NguoiDung.AnyAsync(x => x.Email == email, ct))
            return KetQua<NguoiDungDto>.Loi(StatusCodes.Status409Conflict, "Email đã được sử dụng.");

        if (req.MaLoaiDa is int maLoaiDa && !await db.LoaiDa.AnyAsync(x => x.MaLoaiDa == maLoaiDa, ct))
            return KetQua<NguoiDungDto>.Loi(StatusCodes.Status400BadRequest, "Loại da không tồn tại.");

        var nd = new NguoiDung
        {
            Email = email,
            MatKhauBam = BCrypt.Net.BCrypt.HashPassword(req.MatKhau, workFactor: 12),
            HoTen = req.HoTen.Trim(),
            SoDienThoai = req.SoDienThoai,
            MaLoaiDa = req.MaLoaiDa,
            VaiTro = VaiTroNguoiDung.KhachHang
        };

        db.NguoiDung.Add(nd);
        await db.SaveChangesAsync(ct);
        return KetQua<NguoiDungDto>.ThanhCong(MapDto(nd));
    }

    private static bool XacThucMatKhau(string matKhau, string hash)
    {
        try { return BCrypt.Net.BCrypt.Verify(matKhau, hash); }
        catch (BCrypt.Net.SaltParseException) { return false; } // hash mẫu không hợp lệ trong dữ liệu seed
    }

    private static NguoiDungDto MapDto(NguoiDung x) => new(x.MaNguoiDung, x.Email, x.HoTen, x.VaiTro, x.MaLoaiDa);
}
