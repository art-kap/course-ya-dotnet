using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Repository;

public class InMemoryBookingStore : IBookingRepository
{
    private readonly List<Booking> _bookings = [];

    public void Add(Booking booking)
    {
        _bookings.Add(booking);
    }

    public Booking? FindById(Guid bookingId)
    {
        return _bookings.Find(b => b.Id == bookingId);
    }
}
