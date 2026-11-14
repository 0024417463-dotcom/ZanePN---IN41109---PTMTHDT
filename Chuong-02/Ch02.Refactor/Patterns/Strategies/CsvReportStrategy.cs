namespace EduHub.Ch02.Patterns.Strategies;

public class CsvReportStrategy : IReportStrategy
{
    public void Generate(IReadOnlyList<ClubStat> stats, TextWriter output)
    {
        output.WriteLine("Club,Members");
        foreach (var s in stats)
            output.WriteLine($"{Escape(s.ClubName)},{s.MemberCount}");
    }

    // Trường có dấu phẩy, nháy kép hoặc xuống dòng phải được bao trong dấu nháy kép (RFC 4180).
    private static string Escape(string value) =>
        value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0 ? $"\"{value.Replace("\"", "\"\"")}\"" : value;
}
