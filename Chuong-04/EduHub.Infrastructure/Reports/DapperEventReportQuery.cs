using Dapper;
using EduHub.Application.Reports;
using EduHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EduHub.Infrastructure.Reports;

// Dapper: SQL tự viết, tham số hóa (@minRegistered) nên không bị SQL injection.
// Dùng chung kết nối của DbContext, nên không cần cấu hình thêm.
public class DapperEventReportQuery(EduHubDbContext db) : IEventReportQuery
{
    private const string Sql = """
        SELECT e."Name"     AS "EventName",
               c."Name"     AS "ClubName",
               e."Capacity" AS "Capacity",
               COUNT(r."Id") AS "Registered"
        FROM events e
        JOIN clubs c ON c."Id" = e."ClubId"
        LEFT JOIN registrations r ON r."EventId" = e."Id" AND r."Status" <> 'Cancelled'
        GROUP BY e."Id", e."Name", c."Name", e."Capacity"
        HAVING COUNT(r."Id") >= @minRegistered
        ORDER BY "Registered" DESC, "EventName"
        """;

    public async Task<IReadOnlyList<EventReportRow>> GetRegistrationSummaryAsync(
        int minRegistered, CancellationToken ct = default)
    {
        var conn = db.Database.GetDbConnection();
        var rows = await conn.QueryAsync<EventReportRow>(
            new CommandDefinition(Sql, new { minRegistered }, cancellationToken: ct));
        return rows.AsList();
    }
}
