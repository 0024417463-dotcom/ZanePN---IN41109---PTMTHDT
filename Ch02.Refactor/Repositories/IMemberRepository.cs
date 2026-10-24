using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public interface IMemberRepository
{
    void Add(Member member);
    int CountByClub(Guid clubId);
    IReadOnlyList<Member> GetByClub(Guid clubId);
}
