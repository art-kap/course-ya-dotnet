using CourseWebApi.Application.Dto;

namespace CourseWebApi.Application.Interfaces;

public interface IBookingService
{
    Task<BookingInfo> CreateBookingAsync(Guid eventId);

    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId);

    Task TryConfirmBookingAsync(Guid bookingId, CancellationToken ct = default);

    Task RejectBookingAsync(Guid bookingId, CancellationToken ct = default);
}
