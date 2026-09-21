namespace CourseWebApiProject.Models;

public class Event(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = title;
    public string? Description { get; private set; } = description;
    public DateTime StartAt { get; private set; } = startAt;
    public DateTime EndAt { get; private set; } = endAt;
    public int TotalSeats { get; private set; } = totalSeats;
    public int AvailableSeats { get; private set; } = totalSeats;

    public void Update(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
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
