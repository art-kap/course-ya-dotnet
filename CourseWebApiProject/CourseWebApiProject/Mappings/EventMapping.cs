using CourseWebApiProject.Dto;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Mappings;

public static class EventMapping
{
    public static Event ToEvent(this EventRequestDto eventRequestDto)
    {
        return new Event(
            eventRequestDto.Title,
            eventRequestDto.Description,
            eventRequestDto.StartAt!.Value,
            eventRequestDto.EndAt!.Value);
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
