using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.GioHang;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Giỏ hàng của khách hàng đang đăng nhập. Mọi thao tác trả về giỏ hàng mới nhất.</summary>
[Authorize(Policy = ChinhSach.KhachHang)]
[Route("api/gio-hang")]
public class GioHangController(IGioHangService gioHang) : ApiControllerBase
{
    /// <summary>Giỏ hàng kèm đơn giá hiện hành, tồn kho, thành tiền, tạm tính và bảng phí vận chuyển.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(GioHangDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Lay(CancellationToken ct) =>
        Ok(await gioHang.LayAsync(User.LayMaNguoiDung(), ct));

    /// <summary>Thêm biến thể vào giỏ (cộng dồn nếu đã có); tổng không vượt SoLuongTon.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(GioHangDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Them(ThemVaoGioRequest req, CancellationToken ct) =>
        TraVe(await gioHang.ThemAsync(User.LayMaNguoiDung(), req, ct));

    /// <summary>Đặt lại số lượng một dòng (1..SoLuongTon).</summary>
    [HttpPut("{maBienThe:int}")]
    [ProducesResponseType(typeof(GioHangDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CapNhat(int maBienThe, CapNhatGioRequest req, CancellationToken ct) =>
        TraVe(await gioHang.CapNhatAsync(User.LayMaNguoiDung(), maBienThe, req, ct));

    /// <summary>Xóa một dòng khỏi giỏ.</summary>
    [HttpDelete("{maBienThe:int}")]
    [ProducesResponseType(typeof(GioHangDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> XoaDong(int maBienThe, CancellationToken ct) =>
        Ok(await gioHang.XoaDongAsync(User.LayMaNguoiDung(), maBienThe, ct));

    /// <summary>Xóa sạch giỏ hàng.</summary>
    [HttpDelete]
    [ProducesResponseType(typeof(GioHangDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> XoaTatCa(CancellationToken ct) =>
        Ok(await gioHang.XoaTatCaAsync(User.LayMaNguoiDung(), ct));
}
