namespace CourseWebApiProject.Models;

public class Event(string title, string? description, DateTime startAt, DateTime endAt)
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = title;
    public string? Description { get; private set; } = description;
    public DateTime StartAt { get; private set; } = startAt;
    public DateTime EndAt { get; private set; } = endAt;

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}
