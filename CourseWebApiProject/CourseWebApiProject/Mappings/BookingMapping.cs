using CourseWebApiProject.Dto;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Mappings;

public static class BookingMapping
{
    public static BookingInfo ToInfo(this Booking booking)
    {
        return new BookingInfo(
            booking.Id,
            booking.EventId,
            (int)booking.Status);
    }
}
