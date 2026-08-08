namespace CourseWebApiProject.Dto;

public record EventPutDto(
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt);
