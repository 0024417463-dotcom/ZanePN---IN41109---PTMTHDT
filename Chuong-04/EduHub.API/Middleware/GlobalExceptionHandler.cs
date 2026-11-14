using EduHub.Application.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace EduHub.API.Middleware;

// Xử lý lỗi toàn cục: mọi exception chưa bắt đều đi qua đây và trở thành ProblemDetails (RFC 9457).
public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment env) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Không tìm thấy tài nguyên"),
            ConflictException => (StatusCodes.Status409Conflict, "Xung đột dữ liệu"),
            BusinessRuleException => (StatusCodes.Status400BadRequest, "Vi phạm quy tắc nghiệp vụ"),
            _ => (StatusCodes.Status500InternalServerError, "Lỗi hệ thống")
        };

        // Lỗi nghiệp vụ chỉ cần Warning; lỗi bất ngờ mới là Error kèm stack trace
        if (status == StatusCodes.Status500InternalServerError)
            logger.LogError(ex, "Lỗi chưa xử lý tại {Method} {Path}", ctx.Request.Method, ctx.Request.Path);
        else
            logger.LogWarning("{Title}: {Detail}", title, ex.Message);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            // 500: không lộ chi tiết nội bộ ra ngoài (trừ môi trường Development)
            Detail = status == StatusCodes.Status500InternalServerError && !env.IsDevelopment()
                ? "Đã xảy ra lỗi. Vui lòng thử lại sau."
                : ex.Message,
            Instance = ctx.Request.Path
        };
        problem.Extensions["traceId"] = ctx.TraceIdentifier;

        ctx.Response.StatusCode = status;
        await ctx.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", ct);
        return true;
    }
}
