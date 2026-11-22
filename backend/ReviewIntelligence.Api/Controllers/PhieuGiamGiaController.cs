using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.DonHang;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

[Authorize]
[Route("api/phieu-giam-gia")]
public class PhieuGiamGiaController(IPhieuGiamGiaService phieu) : ApiControllerBase
{
    /// <summary>
    /// Kiểm tra mã giảm giá cho một mức tiền hàng và trả về số tiền được giảm (chỉ để xem trước –
    /// khi đặt hàng, mã được kiểm tra lại với tiền hàng thực tế của giỏ).
    /// </summary>
    [HttpPost("kiem-tra")]
    [ProducesResponseType(typeof(KiemTraPhieuGiamGiaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> KiemTra(KiemTraPhieuGiamGiaRequest req, CancellationToken ct) =>
        TraVe(await phieu.KiemTraAsync(req, ct));
}
