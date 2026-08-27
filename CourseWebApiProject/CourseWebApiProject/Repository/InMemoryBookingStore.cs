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

    public async Task<IReadOnlyCollection<Booking>> GetAllAsync()
    {
        return _bookings.Values.ToList();
    }

    public async Task UpdateAsync(Booking booking)
    {
        if (_bookings.ContainsKey(booking.Id))
        {
            _bookings[booking.Id] = booking;
        }
    }
}
