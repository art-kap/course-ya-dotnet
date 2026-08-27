using CourseWebApiProject.Models;

namespace CourseWebApiProject.Interfaces;

public interface IBookingRepository
{
    Task AddAsync(Booking booking);

    Task<Booking?> FindByIdAsync(Guid bookingId);

    Task<IReadOnlyCollection<Booking>> GetAllAsync();

    Task UpdateAsync(Booking booking);
}
