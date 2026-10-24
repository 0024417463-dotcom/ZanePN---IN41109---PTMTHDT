namespace EduHub.Ch02.Patterns.Strategies;

/// <summary>Strategy: cách trình bày báo cáo CLB.</summary>
public interface IReportStrategy
{
    void Render(IReadOnlyList<ClubStat> stats);
}
