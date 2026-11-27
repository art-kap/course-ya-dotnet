using CourseWebApi.Application.Dto;

namespace CourseWebApi.Application.Interfaces;

public interface IBookingService
{
    Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId);

    Task<BookingInfo> GetBookingByIdAsync(Guid bookingId, Guid requestUserId, bool isAdmin);

    Task TryConfirmBookingAsync(Guid bookingId, CancellationToken ct = default);

    Task RejectBookingAsync(Guid bookingId, CancellationToken ct = default);

    Task CancelBooking(Guid bookingId, Guid requestUserId, bool isAdmin);
}
