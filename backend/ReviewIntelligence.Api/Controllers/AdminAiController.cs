using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.Ai;
using ReviewIntelligence.Api.DTOs.Common;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;
using ReviewIntelligence.Api.Services.Ai;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Quản trị AI: trạng thái mô hình, đồng bộ phân tích, trace tác tử, bảng điều khiển, thực nghiệm.</summary>
[Authorize(Policy = ChinhSach.QuanTri)]
[Route("api/admin/ai")]
public class AdminAiController(IAiService ai, IAiServiceClient client) : ApiControllerBase
{
    /// <summary>Chuyển tiếp nguyên văn JSON của ai-service; lỗi → ProblemDetails.</summary>
    private IActionResult ChuyenTiep(KetQuaAi kq) =>
        kq.OK
            ? (kq.DuLieu is { } json ? StatusCode(kq.MaHttp, json) : StatusCode(kq.MaHttp))
            : Problem(statusCode: kq.MaHttp, title: kq.Loi ?? "Dịch vụ AI trả về lỗi.");

    // ------------------------------------------------------------------ Trạng thái & đồng bộ

    /// <summary>Mô hình nào chạy được (PhoBERT / LLM zero-shot / few-shot) và embedding đang dùng.</summary>
    [HttpGet("trang-thai")]
    public async Task<IActionResult> TrangThai([FromQuery] bool thuKhoiTao, CancellationToken ct) =>
        ChuyenTiep(await client.GuiAsync(HttpMethod.Get, $"phan-tich/mo-hinh?thu_khoi_tao={thuKhoiTao.ToString().ToLowerInvariant()}", null, ct));

    /// <summary>Phân tích các đánh giá DaDuyet còn thiếu kết quả / vector (tatCa=true: phân tích lại toàn bộ).</summary>
    [HttpPost("dong-bo")]
    public async Task<IActionResult> DongBo([FromQuery] bool tatCa, CancellationToken ct) =>
        ChuyenTiep(await client.GuiAsync(HttpMethod.Post, $"phan-tich/dong-bo?tat_ca={tatCa.ToString().ToLowerInvariant()}", null, ct));

    // ------------------------------------------------------------------ Trace tác tử

    [HttpGet("tac-tu")]
    [ProducesResponseType(typeof(KetQuaPhanTrang<TraLoiTomTatDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DanhSachTrace([FromQuery] TraceQuery query, CancellationToken ct) =>
        Ok(await ai.DanhSachTraLoiAsync(query, ct));

    [HttpGet("tac-tu/{maTinNhan:long}")]
    [ProducesResponseType(typeof(TraceChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChiTietTrace(long maTinNhan, CancellationToken ct) =>
        TraVe(await ai.ChiTietTraceAsync(maTinNhan, ct));

    // ------------------------------------------------------------------ Bảng điều khiển

    /// <summary>Thống kê khía cạnh (vw_ThongKeKhiaCanh) và xu hướng cảm xúc theo ngày (vw_XuHuongCamXuc).</summary>
    [HttpGet("bang-dieu-khien")]
    [ProducesResponseType(typeof(BangDieuKhienDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> BangDieuKhien(
        [FromQuery, Range(1, int.MaxValue)] int? maSanPham,
        [FromQuery, Range(7, 365, ErrorMessage = "Số ngày phải từ 7 đến 365.")] int soNgay = 30,
        CancellationToken ct = default) =>
        Ok(await ai.BangDieuKhienAsync(maSanPham, soNgay, ct));

    // ------------------------------------------------------------------ Thực nghiệm

    [HttpGet("thuc-nghiem/chi-so")]
    [ProducesResponseType(typeof(ChiSoThucNghiemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChiSo(CancellationToken ct) => Ok(await ai.ChiSoThucNghiemAsync(ct));

    [HttpGet("thuc-nghiem/ket-qua")]
    [ProducesResponseType(typeof(KetQuaPhanTrang<KetQuaMauDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> KetQua([FromQuery] KetQuaThucNghiemQuery query, CancellationToken ct) =>
        Ok(await ai.KetQuaThucNghiemAsync(query, ct));

    /// <summary>Nạp bộ mẫu ai-service/data/mau_kiem_thu.json vào MauKiemThu (bỏ qua mẫu đã có).</summary>
    [HttpPost("thuc-nghiem/nap-mau")]
    public async Task<IActionResult> NapMau(CancellationToken ct) =>
        ChuyenTiep(await client.GuiAsync(HttpMethod.Post, "thuc-nghiem/nap-mau", null, ct));

    /// <summary>Chạy thực nghiệm nền cho các cấu hình mô hình đã chọn (ghi KetQuaThucNghiem).</summary>
    [HttpPost("thuc-nghiem/chay")]
    public async Task<IActionResult> Chay(ChayThucNghiemRequest req, CancellationToken ct) =>
        ChuyenTiep(await client.GuiAsync(HttpMethod.Post, "thuc-nghiem/chay", new { mo_hinh = req.MoHinh }, ct));

    [HttpGet("thuc-nghiem/trang-thai")]
    public async Task<IActionResult> TrangThaiThucNghiem(CancellationToken ct) =>
        ChuyenTiep(await client.GuiAsync(HttpMethod.Get, "thuc-nghiem/trang-thai", null, ct));
}
