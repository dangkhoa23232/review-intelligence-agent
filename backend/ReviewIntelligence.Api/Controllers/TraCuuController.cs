using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.DTOs.TraCuu;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Dữ liệu tra cứu cho bộ lọc và form.</summary>
[AllowAnonymous]
[Route("api")]
public class TraCuuController(ITraCuuService traCuu) : ApiControllerBase
{
    /// <summary>Danh mục (danh sách phẳng, dựng cây qua MaDanhMucCha).</summary>
    [HttpGet("danh-muc")]
    [ProducesResponseType(typeof(IReadOnlyList<DanhMucDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DanhMuc(CancellationToken ct) => Ok(await traCuu.DanhMucAsync(ct));

    /// <summary>Thương hiệu.</summary>
    [HttpGet("thuong-hieu")]
    [ProducesResponseType(typeof(IReadOnlyList<ThuongHieuDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ThuongHieu(CancellationToken ct) => Ok(await traCuu.ThuongHieuAsync(ct));

    /// <summary>Loại da.</summary>
    [HttpGet("loai-da")]
    [ProducesResponseType(typeof(IReadOnlyList<LoaiDaDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoaiDa(CancellationToken ct) => Ok(await traCuu.LoaiDaAsync(ct));

    /// <summary>Khía cạnh / tiêu chí đánh giá.</summary>
    [HttpGet("khia-canh")]
    [ProducesResponseType(typeof(IReadOnlyList<KhiaCanhDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> KhiaCanh(CancellationToken ct) => Ok(await traCuu.KhiaCanhAsync(ct));
}
