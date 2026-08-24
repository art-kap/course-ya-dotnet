using CourseWebApiProject.Models;

namespace CourseWebApiProject.Interfaces;

public interface IEventRepository
{
    void Add(Event @event);

    void Update(Event @event);

    bool RemoveById(Guid eventId);

    IReadOnlyCollection<Event> GetAll();

    Event? FindById(Guid eventId);
}
