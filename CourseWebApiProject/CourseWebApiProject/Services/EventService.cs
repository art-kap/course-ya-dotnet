using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class EventService : IEventService
{
    private List<Event> _events = [];

    public void AddEvent(EventDto eventDto)
    {
        _events.Add(eventDto.ToEvent());
    }

    public void UpdateEvent(EventDto eventDto)
    {
        var eventToUpdate = _events.Find(e => e.Id == eventDto.Id) ?? throw new EventNotFoundException(eventDto.Id);
        eventToUpdate.Update(eventDto.Title, eventDto.Description, eventDto.StartAt, eventDto.EndAt);
    }

    public void RemoveEvent(int eventId)
    {
        var eventToRemove = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        _events.Remove(eventToRemove);
    }

    public List<EventDto> GetAllEvents()
    {
        return _events.Select(e => e.ToDto()).ToList();
    }

    public EventDto GetEvent(int eventId)
    {
        var eventToGet = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        return eventToGet.ToDto();
    }
}
