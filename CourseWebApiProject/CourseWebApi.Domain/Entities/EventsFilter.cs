namespace CourseWebApi.Domain.Entities;

public record EventsFilter(
    string? Title,
    DateTime? From,
    DateTime? To,
    int Page,
    int PageSize);
