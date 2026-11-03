using CourseWebApi.Application.Interfaces;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;
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
            var pattern = EscapeLikePattern(eventsFilter.Title);
            filteredEvents = filteredEvents.Where(e => EF.Functions.ILike(e.Title, $"%{pattern}%"));
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

        try
        {
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync(ct);

            return true;
        }
        catch (DbUpdateException ex)
        {
            if (IsForeignKeyViolation(ex))
            {
                throw new EventHasBookingsException(eventId);
            }

            // Если ошибка другая (например, дубликат ключа или что-то еще) - пробрасываем дальше
            throw;
        }
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        await _context.SaveChangesAsync(ct);
    }

    private bool IsForeignKeyViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException;

        if (inner == null) 
        {
            return false;
        } 

        var message = inner.Message.ToLower();

        return message.Contains("foreign key") ||
               message.Contains("reference constraint") ||
               message.Contains("violates foreign key");
    }

    private string EscapeLikePattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return pattern;
        }

        return pattern
            .Replace(@"\", @"\\")
            .Replace("%", @"\%")
            .Replace("_", @"\_");
    }
}
