using CourseWebApiProject.Dto;

namespace CourseWebApiProject.Interfaces;

public interface IEventService
{
    PaginatedResult GetEventsByQuery(EventsQuery query);

    EventInfo GetEvent(Guid eventId);

    EventInfo AddEvent(EventCreate eventCreate);

    void UpdateEvent(Guid eventId, EventUpdate eventUpdate);

    void RemoveEvent(Guid eventId);
}
