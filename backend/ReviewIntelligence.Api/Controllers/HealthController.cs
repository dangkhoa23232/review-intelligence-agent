using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.Data;

namespace ReviewIntelligence.Api.Controllers;

[AllowAnonymous]
[Route("api/health")]
public class HealthController(AppDbContext db) : ApiControllerBase
{
    /// <summary>Kiểm tra API và kết nối SQL Server.</summary>
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var dbOk = await db.Database.CanConnectAsync(ct);
        var body = new { TrangThai = dbOk ? "ok" : "loi", SqlServer = dbOk, ThoiGian = DateTime.UtcNow };
        return dbOk ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}
