using EduHub.Domain.Abstractions;
using EduHub.Domain.Entities;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Repositories;

public class EfEventRepository(EduHubDbContext db) : IEventRepository
{
    public async Task<(IReadOnlyList<Event> Items, int TotalCount)> SearchAsync(
        Guid? clubId, string? search, int skip, int take, CancellationToken ct = default)
    {
        IQueryable<Event> query = db.Events.AsNoTracking();
        if (clubId is not null) query = query.Where(e => e.ClubId == clubId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e => e.Name.ToLower().Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(e => e.StartTime).ThenBy(e => e.Id)
            .Skip(skip).Take(take).ToListAsync(ct);
        return (items, total);
    }

    public Task<Event?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.Events.FirstOrDefaultAsync(e => e.Id == id, ct);

    public void Add(Event ev) => db.Events.Add(ev);
    public void Remove(Event ev) => db.Events.Remove(ev);
}
