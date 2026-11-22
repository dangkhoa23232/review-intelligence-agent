using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.DonHang;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Đặt hàng và theo dõi đơn của khách hàng đang đăng nhập.</summary>
[Authorize(Policy = ChinhSach.KhachHang)]
[Route("api/don-hang")]
public class DonHangController(IDonHangService donHang) : ApiControllerBase
{
    /// <summary>
    /// Đặt hàng từ giỏ hàng hiện tại (1 transaction): kiểm tra tồn kho, xuất kho FEFO theo LoHang,
    /// áp mã giảm giá, tính phí vận chuyển, lưu DonHang/ChiTietDonHang, xóa giỏ, tạo ThongBao.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DonHangChiTietDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> DatHang(DatHangRequest req, CancellationToken ct) =>
        TraVe(await donHang.DatHangAsync(User.LayMaNguoiDung(), req, ct), StatusCodes.Status201Created);

    /// <summary>Đơn hàng của tôi, mới nhất trước.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(KetQuaPhanTrang<DonHangTomTatDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DanhSach([FromQuery] PhanTrangQuery query, CancellationToken ct) =>
        Ok(await donHang.DanhSachCuaToiAsync(User.LayMaNguoiDung(), query, ct));

    /// <summary>Chi tiết một đơn hàng của tôi.</summary>
    [HttpGet("{maDonHang:int}")]
    [ProducesResponseType(typeof(DonHangChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChiTiet(int maDonHang, CancellationToken ct) =>
        TraVe(await donHang.ChiTietCuaToiAsync(User.LayMaNguoiDung(), maDonHang, ct));

    /// <summary>Hủy đơn đang ChoXacNhan (qua sp_DoiTrangThaiDonHang – tự hoàn kho).</summary>
    [HttpPut("{maDonHang:int}/huy")]
    [ProducesResponseType(typeof(DonHangChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Huy(int maDonHang, CancellationToken ct) =>
        TraVe(await donHang.HuyAsync(User.LayMaNguoiDung(), maDonHang, ct));
}
