using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class EventService(IEventRepository eventRepository) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    public EventInfo AddEvent(EventCreate eventCreate)
    {
        ValidateDateTimes(eventCreate.StartAt!.Value, eventCreate.EndAt!.Value);
        ValidateTotalSeats(eventCreate.TotalSeats!.Value);

        var newEvent = eventCreate.ToEvent();
        _eventRepository.Add(newEvent);

        return newEvent.ToEventInfo();
    }

    public void UpdateEvent(Guid eventId, EventUpdate eventUpdate)
    {
        ValidateDateTimes(eventUpdate.StartAt!.Value, eventUpdate.EndAt!.Value);

        var eventToUpdate = _eventRepository.FindById(eventId) ?? throw new EventNotFoundException(eventId);
        eventToUpdate.Update(eventUpdate.Title, eventUpdate.Description, eventUpdate.StartAt!.Value, eventUpdate.EndAt!.Value);
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
        var eventsArray = currentPageEvents.Select(e => e.ToEventInfo()).ToArray();

        return new PaginatedResult(eventsCount, eventsArray, query.Page, eventsArray.Length);
    }

    public EventInfo GetEvent(Guid eventId)
    {
        var eventToGet = _eventRepository.FindById(eventId) ?? throw new EventNotFoundException(eventId);
        return eventToGet.ToEventInfo();
    }

    private void ValidateDateTimes(DateTime startAt, DateTime endAt)
    {
        if (startAt >= endAt)
        {
            throw new ArgumentException("Точное время окончания должно быть позже времени начала.");
        }
    }

    private void ValidateTotalSeats(int totalSeats)
    {
        if (totalSeats <= 0)
        {
            throw new ArgumentException("Количество мест должно быть больше нуля.");
        }
    }
}
