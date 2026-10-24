using EduHub.Ch02.Domain;
using EduHub.Ch02.Logging;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

/// <summary>
/// Tạo và quản lý sự kiện. Không biết ai sẽ được báo: chỉ gọi OnEventCreated
/// trên các observer đã đăng ký (Observer pattern).
/// </summary>
public class EventService : IEventService
{
    private readonly IClubRepository _clubs;
    private readonly IEventRepository _events;
    private readonly IAppLogger _logger;
    private readonly List<IEventObserver> _observers;

    public EventService(
        IClubRepository clubs,
        IEventRepository events,
        IAppLogger logger,
        IEnumerable<IEventObserver> observers)
    {
        _clubs = clubs;
        _events = events;
        _logger = logger;
        _observers = observers.ToList();
    }

    public void Subscribe(IEventObserver observer) => _observers.Add(observer);

    public Event CreateEvent(Guid clubId, string title, DateTime date)
    {
        var club = _clubs.GetById(clubId)
            ?? throw new InvalidOperationException("Club not found");

        var ev = new Event { Id = Guid.NewGuid(), ClubId = clubId, Title = title, Date = date };
        _events.Add(ev);

        foreach (var observer in _observers)
            observer.OnEventCreated(ev, club);
        return ev;
    }

    public bool Register(Guid eventId, Guid memberId) => GetEvent(eventId).Register(memberId);

    public void CheckIn(Guid eventId, Guid memberId) => GetEvent(eventId).CheckIn(memberId);

    private Event GetEvent(Guid eventId) =>
        _events.GetById(eventId) ?? throw new InvalidOperationException("Event not found");
}
