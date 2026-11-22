using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.Services;

namespace ReviewIntelligence.Api.Controllers;

/// <summary>Controller gốc. Mỗi controller con tự khai báo [Route("api/...")] theo kebab-case.</summary>
[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Chuyển KetQua thành mã thành công hoặc ProblemDetails với mã lỗi tương ứng.</summary>
    protected IActionResult TraVe<T>(KetQua<T> kq, int maThanhCong = StatusCodes.Status200OK) =>
        kq.OK
            ? StatusCode(maThanhCong, kq.DuLieu)
            : Problem(statusCode: kq.MaLoi, title: kq.ThongBao);
}
