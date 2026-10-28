using CourseWebApi.Application.Dto;

namespace CourseWebApi.Application.Interfaces;

public interface IBookingService
{
    Task<BookingInfo> CreateBookingAsync(Guid eventId);

    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId);

    Task ProcessBookingAsync(Guid bookingId, CancellationToken ct = default);
}
