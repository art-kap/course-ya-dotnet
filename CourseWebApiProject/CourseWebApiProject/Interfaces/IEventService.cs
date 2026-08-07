using CourseWebApiProject.Dto;

namespace CourseWebApiProject.Interfaces;

public interface IEventService
{
    List<EventDto> GetAllEvents();

    EventDto GetEvent(int eventId);

    void AddEvent(EventDto eventDto);

    void UpdateEvent(EventDto eventDto);

    void RemoveEvent(int eventId);
}
