using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Repository;

public class InMemoryEventStore : IEventRepository
{
    private readonly List<Event> _events = [];

    public void Add(Event @event)
    {
        _events.Add(@event);
    }

    public Event? FindById(Guid eventId)
    {
        return _events.Find(e => e.Id ==  eventId);
    }

    public IReadOnlyCollection<Event> GetAll()
    {
        return _events;
    }

    public bool RemoveById(Guid eventId)
    {
        var eventToRemove = FindById(eventId);

        if (eventToRemove == null)
        {
            return false;
        }

        return _events.Remove(eventToRemove);
    }

    public void Update(Event @event)
    {
        var index = _events.FindIndex(e => e.Id == @event.Id);

        if (index != -1) 
        {
            _events[index] = @event;
        }
    }
}
