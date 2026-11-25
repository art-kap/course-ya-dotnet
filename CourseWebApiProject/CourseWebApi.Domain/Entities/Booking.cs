using CourseWebApi.Domain.Exceptions;

namespace CourseWebApi.Domain.Entities;

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
    public Guid UserId { get; private set; }
    public User User { get; private set; } = null!;

    public static Booking Create(Guid eventId, Guid userId)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.Now.ToUniversalTime(),
        };
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.Now.ToUniversalTime();
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.Now.ToUniversalTime();
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            throw new BookingIsAlreadyCancelledException(Id);
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.Now.ToUniversalTime();
    }
}
