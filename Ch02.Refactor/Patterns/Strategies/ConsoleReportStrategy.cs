namespace EduHub.Ch02.Patterns.Strategies;

public class ConsoleReportStrategy : IReportStrategy
{
    private readonly TextWriter _out;
    public ConsoleReportStrategy(TextWriter? output = null) => _out = output ?? Console.Out;

    public void Render(IReadOnlyList<ClubStat> stats)
    {
        foreach (var s in stats)
            _out.WriteLine($"Club: {s.ClubName} | Members: {s.MemberCount}");
    }
}
