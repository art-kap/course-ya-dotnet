using CourseWebApiProject.Dto;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Mappings;

public static class EventMapping
{
    public static Event ToEvent(this EventDto eventDto)
    {
        return new Event
        {
            Id = eventDto.Id,
            Title = eventDto.Title,
            Description = eventDto.Description,
            StartAt = eventDto.StartAt,
            EndAt = eventDto.EndAt
        };
    }

    public static EventDto ToDto(this Event @event)
    {
        return new EventDto(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.StartAt,
            @event.EndAt);
    }

    public static EventDto ToDto(this EventPutDto eventPutDto, int id)
    {
        return new EventDto(
            id,
            eventPutDto.Title,
            eventPutDto.Description,
            eventPutDto.StartAt,
            eventPutDto.EndAt);
    }
}
