using EduHub.Ch02.Domain;
using EduHub.Ch02.Patterns.Observers;
using EduHub.Ch02.Repositories;

namespace EduHub.Ch02.Services;

/// <summary>Chỉ tạo sự kiện rồi báo cho các observer; không biết observer làm gì (SRP, OCP).</summary>
public class EventService : IEventService
{
    private readonly IClubRepository _clubs;
    private readonly List<IEventObserver> _observers = new();

    public EventService(IClubRepository clubs) => _clubs = clubs;

    public void Subscribe(IEventObserver observer) => _observers.Add(observer);

    public Event CreateEvent(Guid clubId, string title, DateTime date)
    {
        var club = _clubs.GetById(clubId) ?? throw new InvalidOperationException("Club not found");
        var ev = new Event { Id = Guid.NewGuid(), ClubId = clubId, Title = title, Date = date };
        foreach (var observer in _observers)
            observer.OnEventCreated(ev, club);
        return ev;
    }
}
