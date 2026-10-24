using EduHub.Ch02.Logging;
using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

/// <summary>Gom số liệu CLB rồi giao cho strategy trình bày; thêm định dạng mới không phải sửa class này.</summary>
public class ReportService : IReportService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;
    private readonly IAppLogger _logger;

    public ReportService(IClubRepository clubs, IMemberRepository members, IAppLogger logger)
    {
        _clubs = clubs;
        _members = members;
        _logger = logger;
    }

    public void Generate(IReportStrategy strategy)
    {
        var stats = _clubs.GetAll()
            .Select(c => new ClubStat(c.Name, _members.CountByClub(c.Id)))
            .ToList();

        strategy.Render(stats);
        _logger.Info($"Report generated for {stats.Count} club(s)");
    }
}
