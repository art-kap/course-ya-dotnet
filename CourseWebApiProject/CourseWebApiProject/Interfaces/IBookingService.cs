using CourseWebApiProject.Dto;

namespace CourseWebApiProject.Interfaces;

public interface IBookingService
{
    Task<BookingInfo> CreateBookingAsync(Guid eventId);

    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId);
}
