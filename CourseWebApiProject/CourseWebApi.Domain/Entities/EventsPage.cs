namespace CourseWebApi.Domain.Entities;

public record EventsPage(
    int EventsCount,
    IReadOnlyCollection<Event> CurrentPageEvents,
    int CurrentPageNumber,
    int CurrentPageSize);
