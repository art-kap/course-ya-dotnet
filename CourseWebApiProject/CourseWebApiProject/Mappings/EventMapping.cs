using CourseWebApiProject.Dto;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Mappings;

public static class EventMapping
{
    public static Event ToEvent(this EventCreate eventCreate)
    {
        return new Event(
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
}
