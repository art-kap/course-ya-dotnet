using CourseWebApiProject.Models;

namespace CourseWebApiProject.Interfaces;

public interface IBookingRepository
{
    void Add(Booking booking);

    Booking? FindById(Guid bookingId);
}
