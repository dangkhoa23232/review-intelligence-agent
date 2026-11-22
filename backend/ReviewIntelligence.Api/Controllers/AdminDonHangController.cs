using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.DonHang;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Quản trị đơn hàng – chỉ tài khoản QuanTri.</summary>
[Authorize(Policy = ChinhSach.QuanTri)]
[Route("api/admin/don-hang")]
public class AdminDonHangController(IDonHangService donHang) : ApiControllerBase
{
    /// <summary>Danh sách đơn (lọc theo TrangThai) kèm số lượng từng trạng thái và các trạng thái được phép chuyển tiếp.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DanhSachDonHangQuanTriDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DanhSach([FromQuery] DonHangQuanTriQuery query, CancellationToken ct) =>
        Ok(await donHang.DanhSachQuanTriAsync(query, ct));

    /// <summary>
    /// Chuyển trạng thái qua dbo.sp_DoiTrangThaiDonHang:
    /// ChoXacNhan → DaXacNhan → DangDongGoi → DangGiao → HoanTat; hủy (DaHuy) được trước khi giao.
    /// </summary>
    [HttpPut("{maDonHang:int}/trang-thai")]
    [ProducesResponseType(typeof(DonHangChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DoiTrangThai(int maDonHang, DoiTrangThaiDonHangRequest req, CancellationToken ct) =>
        TraVe(await donHang.DoiTrangThaiQuanTriAsync(User.LayMaNguoiDung(), maDonHang, req, ct));
}
