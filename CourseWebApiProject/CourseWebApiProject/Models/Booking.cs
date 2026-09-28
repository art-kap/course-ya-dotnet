namespace CourseWebApiProject.Models;

public class Booking
{
    private Booking()
    {
    }

    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public Event Event { get; private set; } = null!;


    public static Booking Create(Guid eventId)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now
        };
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.Now;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.Now;
    }
}
