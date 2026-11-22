using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ReviewIntelligence.Api.Models.Entities;

namespace ReviewIntelligence.Api.Data;

/// <summary>Bao các thủ tục lưu trữ trong schema.</summary>
public partial class AppDbContext
{
    /// <summary>
    /// Gọi dbo.sp_DoiTrangThaiDonHang. Thủ tục tự kiểm tra luồng trạng thái
    /// (ChoXacNhan → DaXacNhan → DangDongGoi → DangGiao → HoanTat; hủy được trước khi giao),
    /// hoàn kho khi DaHuy, ghi ThongBao + NhatKyHeThong trong cùng transaction.
    /// </summary>
    /// <exception cref="NghiepVuException">
    /// 404 khi không tìm thấy đơn (THROW 50001); 400 khi chuyển trạng thái sai luồng (THROW 50002).
    /// </exception>
    public async Task DoiTrangThaiDonHangAsync(int maDonHang, string trangThaiMoi, int? maNguoiDoi, CancellationToken ct = default)
    {
        if (!TrangThaiDonHang.TatCa.Contains(trangThaiMoi))
            throw new NghiepVuException(StatusCodes.Status400BadRequest, $"Trạng thái '{trangThaiMoi}' không hợp lệ.");

        try
        {
            await Database.ExecuteSqlInterpolatedAsync(
                $"EXEC dbo.sp_DoiTrangThaiDonHang @MaDonHang = {maDonHang}, @TrangThaiMoi = {trangThaiMoi}, @MaNguoiDoi = {maNguoiDoi}",
                ct);
        }
        catch (SqlException ex) when (ex.Number == 50001)
        {
            throw new NghiepVuException(StatusCodes.Status404NotFound, ex.Message, ex);
        }
        catch (SqlException ex) when (ex.Number == 50002)
        {
            throw new NghiepVuException(StatusCodes.Status400BadRequest, ex.Message, ex);
        }
    }
}

/// <summary>Lỗi nghiệp vụ có mã HTTP – GlobalExceptionHandler chuyển thành ProblemDetails.</summary>
public class NghiepVuException(int maHttp, string message, Exception? inner = null) : Exception(message, inner)
{
    public int MaHttp { get; } = maHttp;
}
