using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.Services;

public class EventService(AppDbContext context) : IEventService
{
    private readonly AppDbContext _context = context;

    public async Task<EventInfo> AddEvent(EventCreate eventCreate)
    {
        var newEvent = eventCreate.ToEvent();
        await _context.Events.AddAsync(newEvent);
        await _context.SaveChangesAsync();

        return newEvent.ToEventInfo();
    }

    public async Task UpdateEvent(Guid eventId, EventUpdate eventUpdate)
    {
        var eventToUpdate = await _context.Events.FindAsync(eventId) ?? throw new EventNotFoundException(eventId);
        eventToUpdate.Update(eventUpdate.Title, eventUpdate.Description, eventUpdate.StartAt!.Value, eventUpdate.EndAt!.Value);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveEvent(Guid eventId)
    {
        var eventToRemove = await _context.Events.FindAsync(eventId) ?? throw new EventNotFoundException(eventId);
        _context.Events.Remove(eventToRemove);
        await _context.SaveChangesAsync();
    }

    public async Task<PaginatedResult> GetEventsByQuery(EventsQuery query)
    {
        IEnumerable<Event> filteredEvents = await _context.Events.ToListAsync();

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

    public async Task<EventInfo> GetEvent(Guid eventId)
    {
        var eventToGet = await _context.Events.FindAsync(eventId) ?? throw new EventNotFoundException(eventId);
        return eventToGet.ToEventInfo();
    }
}
