using System.Text.Json;
using EduHub.API.Middleware;
using EduHub.Application.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EduHub.Tests.Api;

public class GlobalExceptionHandlerTests
{
    private static async Task<(int Status, string ContentType, JsonElement Body)> RunAsync(Exception ex, string env)
    {
        var host = new Mock<IHostEnvironment>();
        host.SetupGet(h => h.EnvironmentName).Returns(env);
        var handler = new GlobalExceptionHandler(NullLogger<GlobalExceptionHandler>.Instance, host.Object);

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = "/api/events/1";
        ctx.Response.Body = new MemoryStream();

        Assert.True(await handler.TryHandleAsync(ctx, ex, CancellationToken.None));

        ctx.Response.Body.Position = 0;
        var body = await JsonDocument.ParseAsync(ctx.Response.Body);
        return (ctx.Response.StatusCode, ctx.Response.ContentType!, body.RootElement.Clone());
    }

    [Theory]
    [InlineData(typeof(NotFoundException), 404)]
    [InlineData(typeof(ConflictException), 409)]
    [InlineData(typeof(BusinessRuleException), 400)]
    public async Task AppExceptions_MapToProblemDetailsWithExpectedStatus(Type type, int status)
    {
        var ex = (Exception)Activator.CreateInstance(type, "thông báo lỗi")!;
        var (code, contentType, body) = await RunAsync(ex, Environments.Production);

        Assert.Equal(status, code);
        Assert.StartsWith("application/problem+json", contentType);
        Assert.Equal(status, body.GetProperty("status").GetInt32());
        Assert.Equal("thông báo lỗi", body.GetProperty("detail").GetString());
        Assert.True(body.TryGetProperty("traceId", out _));
    }

    [Fact]
    public async Task UnexpectedException_InProduction_HidesInternalDetail()
    {
        var (code, _, body) = await RunAsync(new InvalidOperationException("mật khẩu DB là ..."), Environments.Production);

        Assert.Equal(500, code);
        Assert.DoesNotContain("mật khẩu", body.GetProperty("detail").GetString());
    }
}
