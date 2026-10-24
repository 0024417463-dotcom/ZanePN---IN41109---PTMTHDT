using EduHub.Ch02.Patterns.Strategies;

namespace EduHub.Ch02.Services;

public interface IReportService
{
    void Generate(IReportStrategy strategy);
}
