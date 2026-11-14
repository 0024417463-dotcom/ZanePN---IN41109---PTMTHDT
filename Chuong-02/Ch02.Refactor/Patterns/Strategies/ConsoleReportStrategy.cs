namespace EduHub.Ch02.Patterns.Strategies;

public class ConsoleReportStrategy : IReportStrategy
{
    public void Generate(IReadOnlyList<ClubStat> stats, TextWriter output)
    {
        foreach (var s in stats)
            output.WriteLine($"Club: {s.ClubName} | Members: {s.MemberCount}");
    }
}
