namespace EduHub.Ch02.Patterns.Strategies;

/// <summary>Strategy: mỗi định dạng báo cáo là một class riêng, thêm định dạng mới không phải sửa ReportService (OCP).</summary>
public interface IReportStrategy
{
    void Generate(IReadOnlyList<ClubStat> stats, TextWriter output);
}
