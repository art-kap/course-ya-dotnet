using CourseWebApi.Domain.Entities;
using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.Repository;

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
}
