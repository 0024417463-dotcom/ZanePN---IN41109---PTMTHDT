using EduHub.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Tests.Support;

// SQLite in-memory: một kết nối giữ mở thì database sống đến hết test.
// Đủ để kiểm tra truy vấn EF và SQL thuần của Dapper (không cần PostgreSQL thật).
public sealed class SqliteDb : IDisposable
{
    private readonly SqliteConnection _conn = new("DataSource=:memory:");
    public EduHubDbContext Db { get; }

    public SqliteDb()
    {
        _conn.Open();
        Db = new EduHubDbContext(new DbContextOptionsBuilder<EduHubDbContext>().UseSqlite(_conn).Options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _conn.Dispose();
    }
}
