using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public class InMemoryClubRepository : IClubRepository
{
    private readonly List<Club> _clubs = new();

    public void Add(Club club) => _clubs.Add(club);
    public Club? GetById(Guid id) => _clubs.FirstOrDefault(c => c.Id == id);
    public IReadOnlyList<Club> GetAll() => _clubs.AsReadOnly();
}
