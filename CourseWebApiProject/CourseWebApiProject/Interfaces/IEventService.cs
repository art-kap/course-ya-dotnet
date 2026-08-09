using CourseWebApiProject.Dto;

namespace CourseWebApiProject.Interfaces;

public interface IEventService
{
    List<EventResponseDto> GetAllEvents();

    EventResponseDto GetEvent(Guid eventId);

    EventResponseDto AddEvent(EventRequestDto eventDto);

    void UpdateEvent(Guid eventId, EventRequestDto eventDto);

    void RemoveEvent(Guid eventId);
}
