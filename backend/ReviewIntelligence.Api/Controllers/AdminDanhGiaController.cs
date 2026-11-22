using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.DanhGia;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Kiểm duyệt đánh giá – chỉ tài khoản QuanTri.</summary>
[Authorize(Policy = ChinhSach.QuanTri)]
[Route("api/admin/danh-gia")]
public class AdminDanhGiaController(IDanhGiaService danhGia) : ApiControllerBase
{
    /// <summary>Danh sách đánh giá theo trạng thái (ChoDuyet / DaDuyet / DaAn) kèm số lượng từng trạng thái.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DanhSachDanhGiaQuanTriDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> DanhSach([FromQuery] DanhGiaQuanTriQuery query, CancellationToken ct) =>
        Ok(await danhGia.DanhSachQuanTriAsync(query, ct));

    /// <summary>
    /// Duyệt (DaDuyet) hoặc ẩn (DaAn). Trigger trg_DanhGia_CapNhatDiem tự tính lại điểm sản phẩm;
    /// khi duyệt, đánh giá được gửi sang ai-service để phân tích.
    /// </summary>
    [HttpPut("{maDanhGia:int}/trang-thai")]
    [ProducesResponseType(typeof(DoiTrangThaiDanhGiaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DoiTrangThai(int maDanhGia, DoiTrangThaiDanhGiaRequest req, CancellationToken ct) =>
        TraVe(await danhGia.DoiTrangThaiAsync(User.LayMaNguoiDung(), maDanhGia, req, ct));
}
