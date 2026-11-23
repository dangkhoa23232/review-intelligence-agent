using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.SanPham;
using ReviewIntelligence.Api.Infrastructure;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Quản trị sản phẩm, biến thể (BienThe) và lô hàng (LoHang) – chỉ tài khoản QuanTri.</summary>
[Authorize(Policy = ChinhSach.QuanTri)]
[Route("api/admin/san-pham")]
public class AdminSanPhamController(IAdminSanPhamService sanPham) : ApiControllerBase
{
    /// <summary>Danh sách sản phẩm (mọi trạng thái) kèm số lượng theo trạng thái, tổng tồn kho.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DanhSachSanPhamQuanTriDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DanhSach([FromQuery] SanPhamQuanTriQuery query, CancellationToken ct) =>
        Ok(await sanPham.DanhSachAsync(query, ct));

    /// <summary>Chi tiết để chỉnh sửa: loại da, mọi biến thể (kể cả ngừng hoạt động) và lô hàng.</summary>
    [HttpGet("{maSanPham:int}")]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChiTiet(int maSanPham, CancellationToken ct) =>
        TraVe(await sanPham.ChiTietAsync(maSanPham, ct));

    /// <summary>Thêm sản phẩm. DuongDan bỏ trống → tự sinh từ tên.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Tao(LuuSanPhamRequest req, CancellationToken ct) =>
        TraVe(await sanPham.TaoAsync(req, ct), StatusCodes.Status201Created);

    /// <summary>Cập nhật thông tin, ảnh, loại da phù hợp. DuongDan bỏ trống → giữ nguyên.</summary>
    [HttpPut("{maSanPham:int}")]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CapNhat(int maSanPham, LuuSanPhamRequest req, CancellationToken ct) =>
        TraVe(await sanPham.CapNhatAsync(maSanPham, req, ct));

    /// <summary>Đổi trạng thái DangBan / NgungBan.</summary>
    [HttpPut("{maSanPham:int}/trang-thai")]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DoiTrangThai(int maSanPham, DoiTrangThaiSanPhamRequest req, CancellationToken ct) =>
        TraVe(await sanPham.DoiTrangThaiAsync(maSanPham, req.TrangThai, ct));

    /// <summary>Thêm biến thể (dung tích / SKU). Tồn kho bắt đầu từ 0 – tăng qua nhập lô hàng.</summary>
    [HttpPost("{maSanPham:int}/bien-the")]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ThemBienThe(int maSanPham, LuuBienTheRequest req, CancellationToken ct) =>
        TraVe(await sanPham.ThemBienTheAsync(maSanPham, req, ct), StatusCodes.Status201Created);

    /// <summary>Sửa SKU, dung tích, giá, khuyến mãi, bật/tắt biến thể (không sửa tồn kho trực tiếp).</summary>
    [HttpPut("{maSanPham:int}/bien-the/{maBienThe:int}")]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CapNhatBienThe(int maSanPham, int maBienThe, LuuBienTheRequest req, CancellationToken ct) =>
        TraVe(await sanPham.CapNhatBienTheAsync(maSanPham, maBienThe, req, ct));

    /// <summary>Nhập lô hàng: tạo LoHang và cộng SoLuongNhap vào BienThe.SoLuongTon.</summary>
    [HttpPost("{maSanPham:int}/bien-the/{maBienThe:int}/lo-hang")]
    [ProducesResponseType(typeof(SanPhamQuanTriChiTietDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> NhapLoHang(int maSanPham, int maBienThe, NhapLoHangRequest req, CancellationToken ct) =>
        TraVe(await sanPham.NhapLoHangAsync(maSanPham, maBienThe, req, ct), StatusCodes.Status201Created);
}

/// <summary>Tải ảnh sản phẩm lên thư mục frontend/public/images/san-pham.</summary>
[Authorize(Policy = ChinhSach.QuanTri)]
[Route("api/admin/upload-anh")]
public class AdminUploadAnhController(IAnhSanPhamService anh) : ApiControllerBase
{
    /// <summary>multipart/form-data, trường "tep": ảnh .jpg/.png/.webp. Trả đường dẫn /images/san-pham/&lt;ten-file&gt;.</summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(21 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 21 * 1024 * 1024)]
    [ProducesResponseType(typeof(AnhDaTaiLenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TaiLen(IFormFile? tep, CancellationToken ct) =>
        TraVe(await anh.LuuAsync(tep, ct), StatusCodes.Status201Created);
}
