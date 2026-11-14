using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EduHub.API.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EduHub.Tests.Integration;

/// <summary>
/// Không đặt connection string -> API dùng EF Core in-memory, không cần PostgreSQL.
/// Secret JWT truyền qua UseSetting; dữ liệu mẫu (admin, member) do DbSeeder tạo khi khởi động.
/// Đăng nhập một lần cho cả lớp test (endpoint /api/auth/login bị rate limit 10 lần/phút).
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public string AdminToken { get; private set; } = "";
    public string MemberToken { get; private set; } = "";

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        => builder.UseSetting("JwtSettings:Secret", "chuoi-bi-mat-chi-dung-cho-test-it-nhat-32-ky-tu");

    public async Task InitializeAsync()
    {
        AdminToken = await LoginAsync("admin@eduhub.local", "Admin@123");
        MemberToken = await LoginAsync("member@eduhub.local", "Member@123");
    }

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;

    private async Task<string> LoginAsync(string email, string password)
    {
        var res = await CreateClient().PostAsJsonAsync("/api/auth/login", new { email, password });
        res.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await res.Content.ReadFromJsonAsync<LoginBody>())!.AccessToken;
    }

    public HttpClient ClientWith(string? token)
    {
        var client = CreateClient();
        if (token is not null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    private record LoginBody(string AccessToken);
}

public class ClubsApiTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly HttpClient _anon = factory.ClientWith(null);
    private readonly HttpClient _admin = factory.ClientWith(factory.AdminToken);
    private readonly HttpClient _member = factory.ClientWith(factory.MemberToken);

    [Fact]
    public async Task GetClubs_WithToken_Returns200()
    {
        var res = await _member.GetAsync("/api/clubs");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetClubs_WithoutToken_Returns401()
    {
        var res = await _anon.GetAsync("/api/clubs");

        res.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostClub_AsAdminWithValidBody_Returns201AndClub()
    {
        var res = await _admin.PostAsJsonAsync("/api/clubs", new CreateClubRequest("CLB Test", "mô tả"));

        res.StatusCode.Should().Be(HttpStatusCode.Created);
        (await res.Content.ReadAsStringAsync()).Should().Contain("CLB Test");
        res.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostClub_AsMember_Returns403()
    {
        var res = await _member.PostAsJsonAsync("/api/clubs", new CreateClubRequest("CLB Lậu", null));

        res.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PostClub_WithEmptyName_Returns400()
    {
        var res = await _admin.PostAsJsonAsync("/api/clubs", new CreateClubRequest("", null));

        res.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetClubById_WhenNotExists_Returns404()
    {
        var res = await _admin.GetAsync($"/api/clubs/{Guid.NewGuid()}");

        res.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Healthz_WithoutToken_Returns200()
    {
        var res = await _anon.GetAsync("/healthz");

        res.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
