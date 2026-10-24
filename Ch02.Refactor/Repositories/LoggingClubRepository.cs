using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;

namespace EduHub.Ch02.Repositories;

/// <summary>
/// Decorator: bọc một <see cref="IClubRepository"/> và ghi log mỗi lần truy cập,
/// không phải sửa repository gốc (OCP).
/// </summary>
public class LoggingClubRepository : IClubRepository
{
    private readonly IClubRepository _inner;
    private readonly IAppLogger _logger;

    public LoggingClubRepository(IClubRepository inner, IAppLogger logger)
    {
        _inner = inner;
        _logger = logger;
    }

    public void Add(Club club)
    {
        _logger.Info($"ClubRepository.Add: {club.Name}");
        _inner.Add(club);
    }

    public Club? GetById(Guid id)
    {
        _logger.Info($"ClubRepository.GetById: {id}");
        return _inner.GetById(id);
    }

    public IReadOnlyList<Club> GetAll()
    {
        _logger.Info("ClubRepository.GetAll");
        return _inner.GetAll();
    }
}
