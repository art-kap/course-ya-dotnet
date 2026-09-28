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
        ValidateDateTimes(startAt, endAt);
        ValidateTotalSeats(totalSeats);

        return new Event(title, description, startAt, endAt, totalSeats);
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        ValidateDateTimes(startAt, endAt);

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

    private static void ValidateDateTimes(DateTime startAt, DateTime endAt)
    {
        if (startAt >= endAt)
        {
            throw new ArgumentException("Точное время окончания должно быть позже времени начала.");
        }
    }

    private static void ValidateTotalSeats(int totalSeats)
    {
        if (totalSeats <= 0)
        {
            throw new ArgumentException("Количество мест должно быть больше нуля.");
        }
    }
}
