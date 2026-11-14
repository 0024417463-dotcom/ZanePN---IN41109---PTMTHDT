using EduHub.Ch02.Patterns.Strategies;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

/// <summary>Gom số liệu, còn định dạng đầu ra giao cho <see cref="IReportStrategy"/>.</summary>
public class ReportService : IReportService
{
    private readonly IClubRepository _clubs;
    private readonly IMemberRepository _members;

    public ReportService(IClubRepository clubs, IMemberRepository members)
    {
        _clubs = clubs;
        _members = members;
    }

    public void Generate(IReportStrategy strategy, TextWriter output)
    {
        var stats = _clubs.GetAll()
            .Select(c => new ClubStat(c.Name, _members.CountByClub(c.Id)))
            .ToList();
        strategy.Generate(stats, output);
    }
}
