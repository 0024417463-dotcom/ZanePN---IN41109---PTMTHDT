using EduHub.Domain.Entities;
using EduHub.Infrastructure.Repositories;
using EduHub.Tests.Support;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Tests.Infrastructure;

// Chạy trên SQLite in-memory (không cần PostgreSQL). Kiểm tra truy vấn và ràng buộc của DbContext.
public class RepositoryTests : IDisposable
{
    private readonly SqliteDb _sqlite = new();

    public void Dispose() => _sqlite.Dispose();

    [Fact]
    public async Task Seed_CreatesThreeClubsAndTwoEvents_AndIsIdempotent()
    {
        await _sqlite.Db.SeedAsync();
        await _sqlite.Db.SeedAsync();

        Assert.Equal(3, await _sqlite.Db.Clubs.CountAsync());
        Assert.Equal(2, await _sqlite.Db.Events.CountAsync());
    }

    [Fact]
    public async Task ClubSearch_FiltersOrdersThenPagesInDatabase()
    {
        for (var i = 1; i <= 12; i++) _sqlite.Db.Clubs.Add(Club.Create($"CLB {i:00}", "mô tả"));
        _sqlite.Db.Clubs.Add(Club.Create("Khác", "không khớp từ khóa"));
        await _sqlite.Db.SaveChangesAsync();

        var repo = new EfClubRepository(_sqlite.Db);
        var (items, total) = await repo.SearchAsync("clb", skip: 10, take: 10);

        Assert.Equal(12, total);                       // tổng số bản ghi khớp, trước khi cắt trang
        Assert.Equal(2, items.Count);                  // trang 2 chỉ còn 2 dòng
        Assert.Equal("CLB 11", items[0].Name);         // đã sắp xếp theo tên trước khi Skip
    }

    [Fact]
    public async Task EventSearch_FilterByClub()
    {
        var a = Club.Create("A", "");
        var b = Club.Create("B", "");
        _sqlite.Db.Clubs.AddRange(a, b);
        _sqlite.Db.Events.AddRange(
            Event.Create("E1", a.Id, DateTime.UtcNow.AddDays(1), 5),
            Event.Create("E2", a.Id, DateTime.UtcNow.AddDays(2), 5),
            Event.Create("E3", b.Id, DateTime.UtcNow.AddDays(3), 5));
        await _sqlite.Db.SaveChangesAsync();

        var (items, total) = await new EfEventRepository(_sqlite.Db).SearchAsync(a.Id, null, 0, 10);

        Assert.Equal(2, total);
        Assert.All(items, e => Assert.Equal(a.Id, e.ClubId));
    }

    [Fact]
    public async Task Registration_CompositeUniqueIndex_RejectsDuplicate()
    {
        var club = Club.Create("A", "");
        var ev = Event.Create("E", club.Id, DateTime.UtcNow.AddDays(1), 5);
        var member = Member.Create("SV01", "An", "an@x.vn");
        _sqlite.Db.AddRange(club, ev, member);
        _sqlite.Db.Registrations.Add(Registration.Create(member.Id, ev.Id));
        await _sqlite.Db.SaveChangesAsync();

        _sqlite.Db.Registrations.Add(Registration.Create(member.Id, ev.Id));
        await Assert.ThrowsAsync<DbUpdateException>(() => _sqlite.Db.SaveChangesAsync());
    }

    [Fact]
    public async Task UnitOfWork_SavesRepositoryChangesInOneCall()
    {
        var uow = new EfUnitOfWork(_sqlite.Db);
        var repo = new EfClubRepository(_sqlite.Db);

        repo.Add(Club.Create("CLB 1", ""));
        repo.Add(Club.Create("CLB 2", ""));
        Assert.Equal(0, await _sqlite.Db.Clubs.AsNoTracking().CountAsync()); // chưa Save thì DB chưa có gì

        Assert.Equal(2, await uow.SaveChangesAsync());
    }
}
