using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.DanhGia;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

[Route("api/danh-gia")]
public class DanhGiaController(IDanhGiaService danhGia) : ApiControllerBase
{
    /// <summary>Gửi đánh giá (lưu ở trạng thái ChoDuyet). Truyền MaChiTietDH để xác thực đã mua hàng.</summary>
    [HttpPost]
    [Authorize(Policy = ChinhSach.KhachHang)]
    [ProducesResponseType(typeof(DanhGiaDaTaoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Tao(TaoDanhGiaRequest req, CancellationToken ct) =>
        TraVe(await danhGia.TaoAsync(User.LayMaNguoiDung(), req, ct), StatusCodes.Status201Created);

    /// <summary>Các dòng đơn HoanTat của tôi chứa sản phẩm này và chưa được đánh giá.</summary>
    [HttpGet("dong-don-co-the-danh-gia")]
    [Authorize(Policy = ChinhSach.KhachHang)]
    [ProducesResponseType(typeof(IReadOnlyList<DongDonCoTheDanhGiaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DongDonCoTheDanhGia(
        [FromQuery, Range(1, int.MaxValue, ErrorMessage = "Mã sản phẩm không hợp lệ.")] int maSanPham,
        CancellationToken ct) =>
        Ok(await danhGia.DongDonCoTheDanhGiaAsync(User.LayMaNguoiDung(), maSanPham, ct));

    /// <summary>Báo cáo đánh giá không phù hợp (mỗi người chỉ báo cáo 1 lần / đánh giá).</summary>
    [HttpPost("{maDanhGia:int}/bao-cao")]
    [Authorize]
    [ProducesResponseType(typeof(BaoCaoDanhGiaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BaoCao(int maDanhGia, BaoCaoDanhGiaRequest req, CancellationToken ct) =>
        TraVe(await danhGia.BaoCaoAsync(User.LayMaNguoiDung(), maDanhGia, req, ct), StatusCodes.Status201Created);
}
