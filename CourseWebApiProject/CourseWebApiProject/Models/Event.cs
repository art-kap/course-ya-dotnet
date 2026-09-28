namespace CourseWebApiProject.Models;

public class Event
{
    private Event()
    {
    }

    private Event(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = totalSeats;
    }

    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }
    public List<Booking> Bookings { get; private set; } = [];

    public static Event Create(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        return new Event(title, description, startAt, endAt, totalSeats);
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
        {
            return false;
        }

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats += count;
    }
}
