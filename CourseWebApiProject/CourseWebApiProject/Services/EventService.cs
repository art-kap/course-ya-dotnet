using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;
using System.Text.RegularExpressions;

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
        eventToUpdate.Update(eventDto.Title, eventDto.Description, eventDto.StartAt!.Value, eventDto.EndAt!.Value);
    }

    public void RemoveEvent(Guid eventId)
    {
        var eventToRemove = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        _events.Remove(eventToRemove);
    }

    public List<EventResponseDto> GetEventsByQuery(EventsQuery query)
    {
        IEnumerable<Event> filteredEvents = _events;

        if (query.Title != null)
        {
            filteredEvents = filteredEvents.Where(e => Regex.IsMatch(e.Title, query.Title, RegexOptions.IgnoreCase));
        }

        if (query.From != null)
        {
            filteredEvents = filteredEvents.Where(e => e.StartAt >= query.From.Value);
        }

        if (query.To != null)
        {
            filteredEvents = filteredEvents.Where(e => e.EndAt <= query.To.Value);
        }

        return filteredEvents.Select(e => e.ToResponseDto()).ToList();
    }

    public EventResponseDto GetEvent(Guid eventId)
    {
        var eventToGet = _events.Find(e => e.Id == eventId) ?? throw new EventNotFoundException(eventId);
        return eventToGet.ToResponseDto();
    }
}
