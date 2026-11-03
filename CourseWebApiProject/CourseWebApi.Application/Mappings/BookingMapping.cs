using CourseWebApi.Application.Dto;
using CourseWebApi.Domain.Entities;

namespace CourseWebApi.Application.Mappings;

public static class BookingMapping
{
    public static BookingInfo ToInfo(this Booking booking)
    {
        return new BookingInfo(
            booking.Id,
            booking.EventId,
            (int)booking.Status,
            booking.CreatedAt,
            booking.ProcessedAt);
    }
}
