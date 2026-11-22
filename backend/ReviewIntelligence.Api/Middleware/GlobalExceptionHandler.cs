using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ReviewIntelligence.Api.Data;

namespace ReviewIntelligence.Api.Middleware;

/// <summary>Bắt mọi exception chưa xử lý và trả về ProblemDetails (RFC 7807) thống nhất.</summary>
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is NghiepVuException nv)
        {
            context.Response.StatusCode = nv.MaHttp;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = nv.MaHttp,
                Title = nv.Message,
                Instance = context.Request.Path
            }, ct);
            return true;
        }

        logger.LogError(exception, "Lỗi chưa xử lý: {Path}", context.Request.Path);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Đã xảy ra lỗi hệ thống.",
            Detail = env.IsDevelopment() ? exception.Message : null,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = problem.Status.Value;
        await context.Response.WriteAsJsonAsync(problem, ct);
        return true;
    }
}
