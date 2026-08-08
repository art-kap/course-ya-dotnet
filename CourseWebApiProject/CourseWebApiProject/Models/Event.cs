namespace CourseWebApiProject.Models;

public class Event
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt)
    {
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}
