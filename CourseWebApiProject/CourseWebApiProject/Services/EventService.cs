using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class EventService(IEventRepository eventRepository) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    public EventResponseDto AddEvent(EventRequestDto eventDto)
    {
        if (eventDto.StartAt >= eventDto.EndAt)
        {
            throw new ArgumentException("Точное время окончания должно быть позже времени начала.");
        }

        var newEvent = eventDto.ToEvent();
        _eventRepository.Add(newEvent);

        return newEvent.ToResponseDto();
    }

    public void UpdateEvent(Guid eventId, EventRequestDto eventDto)
    {
        if (eventDto.StartAt >= eventDto.EndAt)
        {
            throw new ArgumentException("Точное время окончания должно быть позже времени начала.");
        }

        var eventToUpdate = _eventRepository.FindById(eventId) ?? throw new EventNotFoundException(eventId);
        eventToUpdate.Update(eventDto.Title, eventDto.Description, eventDto.StartAt!.Value, eventDto.EndAt!.Value);
        _eventRepository.Update(eventToUpdate);
    }

    public void RemoveEvent(Guid eventId)
    {
        if (!_eventRepository.RemoveById(eventId))
        {
            throw new EventNotFoundException(eventId);
        }
    }

    public PaginatedResult GetEventsByQuery(EventsQuery query)
    {
        IEnumerable<Event> filteredEvents = _eventRepository.GetAll();

        if (query.Title != null)
        {
            filteredEvents = filteredEvents.Where(e => e.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));
        }

        if (query.From != null)
        {
            filteredEvents = filteredEvents.Where(e => e.StartAt >= query.From.Value);
        }

        if (query.To != null)
        {
            filteredEvents = filteredEvents.Where(e => e.EndAt <= query.To.Value);
        }

        var eventsCount = filteredEvents.Count();
        var currentPageEvents = filteredEvents.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize);
        var eventsArray = currentPageEvents.Select(e => e.ToResponseDto()).ToArray();

        return new PaginatedResult(eventsCount, eventsArray, query.Page, eventsArray.Length);
    }

    public EventResponseDto GetEvent(Guid eventId)
    {
        var eventToGet = _eventRepository.FindById(eventId) ?? throw new EventNotFoundException(eventId);
        return eventToGet.ToResponseDto();
    }
}
