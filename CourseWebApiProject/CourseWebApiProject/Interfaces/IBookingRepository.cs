using CourseWebApiProject.Models;

namespace CourseWebApiProject.Interfaces;

public interface IBookingRepository
{
    Task Add(Booking booking, CancellationToken ct = default);

    Task<Booking?> FindAsync(Guid bookingId, CancellationToken ct = default);

    Task<IReadOnlyCollection<Booking>> GetAll(CancellationToken ct = default);

    Task<IReadOnlyCollection<Booking>> GetPending(CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
