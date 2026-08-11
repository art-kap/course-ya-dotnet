using CourseWebApiProject.Dto;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Mappings;

public static class EventMapping
{
    public static Event ToEvent(this EventRequestDto eventRequestDto)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = eventRequestDto.Title,
            Description = eventRequestDto.Description,
            StartAt = eventRequestDto.StartAt!.Value,
            EndAt = eventRequestDto.EndAt!.Value
        };
    }

    public static EventResponseDto ToResponseDto(this Event @event)
    {
        return new EventResponseDto(
            @event.Id,
            @event.Title,
            @event.Description,
            @event.StartAt,
            @event.EndAt);
    }
}
