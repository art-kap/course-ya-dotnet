using CourseWebApi.Domain.Entities;

namespace CourseWebApiProject.Interfaces;

public interface IEventRepository
{
    Task Add(Event @event, CancellationToken ct = default);

    Task<bool> RemoveById(Guid eventId, CancellationToken ct = default);

    Task<IReadOnlyCollection<Event>> GetAll(CancellationToken ct = default);

    Task<Event?> FindAsync(Guid eventId, CancellationToken ct = default);

    Task<bool> ContainsId(Guid eventId, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
