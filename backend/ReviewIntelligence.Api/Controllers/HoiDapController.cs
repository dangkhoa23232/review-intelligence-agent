using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.Ai;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Hỏi đáp RAG có dẫn chứng về sản phẩm (khách vãng lai hoặc đã đăng nhập).</summary>
[AllowAnonymous]
[Route("api/hoi-dap")]
public class HoiDapController(IAiService ai) : ApiControllerBase
{
    private int? MaNguoiDungHienTai =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var ma) ? ma : null;

    /// <summary>
    /// Đặt câu hỏi. Tác tử: lập kế hoạch → tìm đánh giá (Qdrant) + thống kê khía cạnh (vw_ThongKeKhiaCanh)
    /// → kiểm soát dẫn chứng → trả lời kèm trích dẫn; lưu TinNhan + NhatKyTacTu.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(HoiDapResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Hoi(HoiDapRequest req, CancellationToken ct) =>
        TraVe(await ai.HoiDapAsync(MaNguoiDungHienTai, req, ct));

    /// <summary>Lịch sử tin nhắn của một phiên (chỉ chủ phiên, hoặc phiên ẩn danh).</summary>
    [HttpGet("phien/{maPhien:guid}")]
    [ProducesResponseType(typeof(PhienHoiDapDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Phien(Guid maPhien, CancellationToken ct) =>
        TraVe(await ai.LayPhienAsync(maPhien, MaNguoiDungHienTai, ct));
}
