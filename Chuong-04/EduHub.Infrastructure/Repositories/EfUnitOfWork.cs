using EduHub.Domain.Abstractions;
using EduHub.Infrastructure.Persistence;

namespace EduHub.Infrastructure.Repositories;

// DbContext đã là Unit of Work (theo dõi thay đổi, SaveChanges = một transaction).
// Lớp này chỉ che DbContext sau interface để lớp Application không phụ thuộc EF Core.
public class EfUnitOfWork(EduHubDbContext db) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default) => db.SaveChangesAsync(ct);
}
