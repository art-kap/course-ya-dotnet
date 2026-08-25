using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using System.Collections.Concurrent;

namespace CourseWebApiProject.Repository;

public class InMemoryBookingStore : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public async Task AddAsync(Booking booking)
    {
        _bookings.TryAdd(booking.Id, booking);
    }

    public async Task<Booking?> FindByIdAsync(Guid bookingId)
    {
        _bookings.TryGetValue(bookingId, out var booking);
        return booking;
    }
}
