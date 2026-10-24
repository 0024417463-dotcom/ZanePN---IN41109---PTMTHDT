using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Services;

public interface IEventService
{
    Event CreateEvent(Guid clubId, string title, DateTime date);
    bool Register(Guid eventId, Guid memberId);
    void CheckIn(Guid eventId, Guid memberId);
}
