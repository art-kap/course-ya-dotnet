using CourseWebApiProject.Dto;

namespace CourseWebApiProject.Interfaces;

public interface IEventService
{
    Task<PaginatedResult> GetEventsByQuery(EventsQuery query);

    Task<EventInfo> GetEvent(Guid eventId);

    Task<EventInfo> AddEvent(EventCreate eventCreate);

    Task UpdateEvent(Guid eventId, EventUpdate eventUpdate);

    Task RemoveEvent(Guid eventId);
}
