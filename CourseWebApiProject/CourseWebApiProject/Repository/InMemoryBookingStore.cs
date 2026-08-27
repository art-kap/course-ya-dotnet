using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using System.Collections.Concurrent;

namespace CourseWebApiProject.Repository;

public class InMemoryBookingStore : IBookingRepository
{
    private readonly ConcurrentDictionary<Guid, Booking> _bookings = new();

    public Task AddAsync(Booking booking)
    {
        _bookings.TryAdd(booking.Id, booking);
        return Task.CompletedTask;
    }

    public Task<Booking?> FindByIdAsync(Guid bookingId)
    {
        _bookings.TryGetValue(bookingId, out var booking);
        return Task.FromResult(booking);
    }

    public Task<IReadOnlyCollection<Booking>> GetAllAsync()
    {
        return Task.FromResult<IReadOnlyCollection<Booking>>(_bookings.Values.ToList());
    }

    public Task UpdateAsync(Booking booking)
    {
        if (_bookings.ContainsKey(booking.Id))
        {
            _bookings[booking.Id] = booking;
        }

        return Task.CompletedTask;
    }
}
