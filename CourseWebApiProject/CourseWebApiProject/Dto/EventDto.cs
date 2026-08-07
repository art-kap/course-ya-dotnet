namespace CourseWebApiProject.Dto;

public record EventDto(
    int Id,
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt);
