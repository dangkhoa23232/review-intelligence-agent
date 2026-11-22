using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.DTOs.DanhGia;
using ReviewIntelligence.Api.DTOs.SanPham;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

[AllowAnonymous]
[Route("api/san-pham")]
public class SanPhamController(ISanPhamService sanPham) : ApiControllerBase
{
    /// <summary>
    /// Tìm kiếm sản phẩm đang bán. Ví dụ:
    /// /api/san-pham?tuKhoa=kem&amp;maLoaiDa=1&amp;giaTu=100000&amp;giaDen=500000&amp;sapXep=giaTang&amp;trang=1&amp;kichThuoc=12
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(KetQuaPhanTrang<SanPhamTomTatDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TimKiem([FromQuery] TimKiemSanPhamQuery query, CancellationToken ct) =>
        Ok(await sanPham.TimKiemAsync(query, ct));

    /// <summary>Chi tiết sản phẩm: loại da, biến thể, tóm tắt khía cạnh, trang đầu đánh giá đã duyệt.</summary>
    [HttpGet("{duongDan}")]
    [ProducesResponseType(typeof(SanPhamChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChiTiet([MaxLength(280)] string duongDan, CancellationToken ct) =>
        TraVe(await sanPham.LayChiTietAsync(duongDan, ct));

    /// <summary>Đánh giá đã duyệt của sản phẩm (phân trang).</summary>
    [HttpGet("{duongDan}/danh-gia")]
    [ProducesResponseType(typeof(KetQuaPhanTrang<DanhGiaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DanhGia([MaxLength(280)] string duongDan, [FromQuery] PhanTrangQuery query, CancellationToken ct) =>
        TraVe(await sanPham.LayDanhGiaAsync(duongDan, query, ct));
}
