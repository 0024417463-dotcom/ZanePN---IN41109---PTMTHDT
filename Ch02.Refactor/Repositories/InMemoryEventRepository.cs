using EduHub.Ch02.Domain;

namespace EduHub.Ch02.Repositories;

public class InMemoryEventRepository : IEventRepository
{
    private readonly List<Event> _events = new();

    public void Add(Event ev) => _events.Add(ev);
    public Event? GetById(Guid id) => _events.FirstOrDefault(e => e.Id == id);
}
