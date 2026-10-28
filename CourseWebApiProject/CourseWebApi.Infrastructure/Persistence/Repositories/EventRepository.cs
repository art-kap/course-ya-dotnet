using CourseWebApi.Application.Interfaces;
using CourseWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApi.Infrastructure.Persistence.Repositories;

public class EventRepository(AppDbContext context) : IEventRepository
{
    private readonly AppDbContext _context = context;

    public async Task Add(Event @event, CancellationToken ct = default)
    {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> ContainsId(Guid eventId, CancellationToken ct = default)
    {
        return await _context.Events.AnyAsync(e => e.Id == eventId, ct);
    }

    public async Task<Event?> FindAsync(Guid eventId, CancellationToken ct = default)
    {
        return await _context.Events.FindAsync(eventId, ct);
    }

    public async Task<IReadOnlyCollection<Event>> GetAll(CancellationToken ct = default)
    {
        return await _context.Events.ToListAsync(ct);
    }

    public async Task<EventsPage> GetFiltered(EventsFilter eventsFilter, CancellationToken ct = default)
    {
        var filteredEvents = _context.Events.AsQueryable();

        if (eventsFilter.Title != null)
        {
            filteredEvents = filteredEvents.Where(e => EF.Functions.ILike(e.Title, $"%{eventsFilter.Title}%"));
        }

        if (eventsFilter.From != null)
        {
            filteredEvents = filteredEvents.Where(e => e.StartAt >= eventsFilter.From.Value.ToUniversalTime());
        }

        if (eventsFilter.To != null)
        {
            filteredEvents = filteredEvents.Where(e => e.EndAt <= eventsFilter.To.Value.ToUniversalTime());
        }

        var eventsCount = await filteredEvents.CountAsync(ct);
        var currentPageEvents = await filteredEvents.Skip((eventsFilter.Page - 1) * eventsFilter.PageSize).Take(eventsFilter.PageSize).ToListAsync(ct);

        return new EventsPage(eventsCount, currentPageEvents, eventsFilter.Page, currentPageEvents.Count);
    }

    public async Task<bool> RemoveById(Guid eventId, CancellationToken ct = default)
    {
        var @event = await FindAsync(eventId, ct);

        if (@event == null)
        {
            return false;
        }

        _context.Events.Remove(@event);
        await _context.SaveChangesAsync(ct);
        return true;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> HasBookings(Guid eventId, CancellationToken ct = default)
    {
        return _context.Bookings.Any(b => b.EventId == eventId);
    }
}
