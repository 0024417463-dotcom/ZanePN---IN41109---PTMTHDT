using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;

namespace EduHub.Ch02.Repositories;

/// <summary>Decorator: bọc một IClubRepository và ghi log mỗi lần truy cập, không sửa repository gốc (OCP).</summary>
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
        _logger.Log($"ClubRepository.Add: {club.Name}");
        _inner.Add(club);
    }

    public Club? GetById(Guid id)
    {
        var club = _inner.GetById(id);
        _logger.Log($"ClubRepository.GetById({id}) -> {(club is null ? "not found" : club.Name)}");
        return club;
    }

    public IReadOnlyList<Club> GetAll()
    {
        var clubs = _inner.GetAll();
        _logger.Log($"ClubRepository.GetAll -> {clubs.Count} club(s)");
        return clubs;
    }
}
