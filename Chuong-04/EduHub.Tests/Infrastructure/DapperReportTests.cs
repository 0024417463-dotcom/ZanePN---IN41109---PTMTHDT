using EduHub.Domain.Entities;
using EduHub.Infrastructure.Reports;
using EduHub.Tests.Support;

namespace EduHub.Tests.Infrastructure;

public class DapperReportTests : IDisposable
{
    private readonly SqliteDb _sqlite = new();

    public void Dispose() => _sqlite.Dispose();

    private async Task SeedAsync()
    {
        var club = Club.Create("CLB Lập trình", "");
        var busy = Event.Create("Workshop", club.Id, DateTime.UtcNow.AddDays(1), 4);
        var empty = Event.Create("Hackathon", club.Id, DateTime.UtcNow.AddDays(2), 10);
        var m1 = Member.Create("SV01", "An", "an@x.vn");
        var m2 = Member.Create("SV02", "Bình", "binh@x.vn");
        var m3 = Member.Create("SV03", "Chi", "chi@x.vn");
        var cancelled = Registration.Create(m3.Id, busy.Id);
        cancelled.Cancel();
        _sqlite.Db.AddRange(club, busy, empty, m1, m2, m3,
            Registration.Create(m1.Id, busy.Id), Registration.Create(m2.Id, busy.Id), cancelled);
        await _sqlite.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Summary_CountsActiveRegistrationsPerEvent_IgnoringCancelled()
    {
        await SeedAsync();
        var rows = await new DapperEventReportQuery(_sqlite.Db).GetRegistrationSummaryAsync(0);

        Assert.Equal(2, rows.Count);
        Assert.Equal("Workshop", rows[0].EventName);   // nhiều lượt đăng ký nhất đứng đầu
        Assert.Equal(2, rows[0].Registered);           // đăng ký đã hủy không tính
        Assert.Equal(50.0, rows[0].FillRate);
        Assert.Equal("CLB Lập trình", rows[0].ClubName);
        Assert.Equal(0, rows[1].Registered);           // LEFT JOIN: sự kiện chưa ai đăng ký vẫn có dòng
    }

    [Fact]
    public async Task Summary_MinRegisteredParameter_FiltersRows()
    {
        await SeedAsync();
        var rows = await new DapperEventReportQuery(_sqlite.Db).GetRegistrationSummaryAsync(1);

        Assert.Single(rows);
        Assert.Equal("Workshop", rows[0].EventName);
    }
}
