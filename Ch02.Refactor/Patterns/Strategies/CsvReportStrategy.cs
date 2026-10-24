namespace EduHub.Ch02.Patterns.Strategies;

public class CsvReportStrategy : IReportStrategy
{
    private readonly TextWriter _out;
    public CsvReportStrategy(TextWriter? output = null) => _out = output ?? Console.Out;

    public void Render(IReadOnlyList<ClubStat> stats)
    {
        _out.WriteLine("ClubName,MemberCount");
        foreach (var s in stats)
            _out.WriteLine($"\"{s.ClubName.Replace("\"", "\"\"")}\",{s.MemberCount}");
    }
}
