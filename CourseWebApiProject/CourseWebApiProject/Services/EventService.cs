using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class EventService : IEventService
{
    private List<Event> _events = [];

    public EventResponseDto AddEvent(EventRequestDto eventDto)
    {
        if (eventDto.StartAt >= eventDto.EndAt)
        {
            throw new ArgumentException("Точное время окончания должно быть позже времени начала.");
        }

        var newEvent = eventDto.ToEvent();
        _events.Add(newEvent);

        return newEvent.ToResponseDto();
    }

    public void UpdateEvent(Guid eventId, EventRequestDto eventDto)
    {
        if (eventDto.StartAt >= eventDto.EndAt)
        {
            throw new ArgumentException("Точное время окончания должно быть позже времени начала.");
        }

        var eventToUpdate = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        eventToUpdate.Update(eventDto.Title, eventDto.Description, eventDto.StartAt.GetValueOrDefault(), eventDto.EndAt.GetValueOrDefault());
    }

    public void RemoveEvent(Guid eventId)
    {
        var eventToRemove = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        _events.Remove(eventToRemove);
    }

    public List<EventResponseDto> GetAllEvents()
    {
        return _events.Select(e => e.ToResponseDto()).ToList();
    }

    public EventResponseDto GetEvent(Guid eventId)
    {
        var eventToGet = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        return eventToGet.ToResponseDto();
    }
}
