using CourseWebApi.Application.Dto;
using CourseWebApi.Domain.Entities;

namespace CourseWebApi.Application.Mappings;

public static class EventMapping
{
    public static Event ToEvent(this EventCreate eventCreate)
    {
        return Event.Create(
            eventCreate.Title,
            eventCreate.Description,
            eventCreate.StartAt!.Value,
            eventCreate.EndAt!.Value,
            eventCreate.TotalSeats!.Value);
    }

    public static EventInfo ToEventInfo(this Event @event)
    {
        return new EventInfo(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.StartAt,
            @event.EndAt,
            @event.TotalSeats,
            @event.AvailableSeats);
    }

    public static EventsFilter ToEventsFilter(this EventsQuery eventsQuery)
    {
        return new EventsFilter(
            eventsQuery.Title,
            eventsQuery.From,
            eventsQuery.To,
            eventsQuery.Page,
            eventsQuery.PageSize);
    }

    public static PaginatedResult ToPaginatedResult(this EventsPage eventsPage)
    {
        return new PaginatedResult(
            eventsPage.EventsCount,
            [.. eventsPage.CurrentPageEvents.Select(ToEventInfo)],
            eventsPage.CurrentPageNumber,
            eventsPage.CurrentPageSize);
    }
}
