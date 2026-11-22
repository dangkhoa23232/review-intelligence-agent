using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Data;
using ReviewIntelligence.Api.DTOs.DonHang;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Services;

public interface IPhieuGiamGiaService
{
    Task<KetQua<KiemTraPhieuGiamGiaResponse>> KiemTraAsync(KiemTraPhieuGiamGiaRequest req, CancellationToken ct);
}

public class PhieuGiamGiaService(AppDbContext db) : IPhieuGiamGiaService
{
    private static readonly CultureInfo Vi = CultureInfo.GetCultureInfo("vi-VN");

    /// <summary>
    /// Tính tiền giảm của phiếu cho một mức TienHang. Dùng chung cho API kiểm tra và lúc đặt hàng.
    /// PhanTram: TienHang × GiaTriGiam% (làm tròn đồng), áp trần GiamToiDa; SoTien: GiaTriGiam. Không vượt TienHang.
    /// </summary>
    public static (decimal TienGiam, string? Loi) TinhTienGiam(PhieuGiamGia p, decimal tienHang, DateTime now)
    {
        if (now < p.BatDau)
            return (0, "Mã giảm giá chưa đến thời gian áp dụng.");
        if (now > p.KetThuc)
            return (0, "Mã giảm giá đã hết hạn.");
        if (p.SoLuotConLai is <= 0)
            return (0, "Mã giảm giá đã hết lượt sử dụng.");
        if (tienHang < p.DonToiThieu)
            return (0, $"Đơn hàng tối thiểu {p.DonToiThieu.ToString("N0", Vi)}đ để dùng mã này.");

        decimal giam;
        if (p.KieuGiam == KieuGiam.PhanTram)
        {
            giam = Math.Round(tienHang * p.GiaTriGiam / 100m, 0, MidpointRounding.AwayFromZero);
            if (p.GiamToiDa is decimal tran) giam = Math.Min(giam, tran);
        }
        else
        {
            giam = p.GiaTriGiam;
        }

        return (Math.Min(giam, tienHang), null);
    }

    public async Task<KetQua<KiemTraPhieuGiamGiaResponse>> KiemTraAsync(KiemTraPhieuGiamGiaRequest req, CancellationToken ct)
    {
        var maCode = req.MaCode.Trim();
        var phieu = await db.PhieuGiamGia.AsNoTracking().FirstOrDefaultAsync(x => x.MaCode == maCode, ct);
        if (phieu is null)
            return KetQua<KiemTraPhieuGiamGiaResponse>.Loi(StatusCodes.Status404NotFound, "Mã giảm giá không tồn tại.");

        var (tienGiam, loi) = TinhTienGiam(phieu, req.TienHang, DateTime.UtcNow);
        if (loi is not null)
            return KetQua<KiemTraPhieuGiamGiaResponse>.Loi(StatusCodes.Status400BadRequest, loi);

        return KetQua<KiemTraPhieuGiamGiaResponse>.ThanhCong(new KiemTraPhieuGiamGiaResponse(
            phieu.MaCode, phieu.KieuGiam, phieu.GiaTriGiam, phieu.GiamToiDa, phieu.DonToiThieu,
            phieu.KetThuc, req.TienHang, tienGiam));
    }
}
