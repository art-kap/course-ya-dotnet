namespace CourseWebApiProject.Dto;

/// <summary>
/// Параметры фильтрации событий
/// </summary>
/// <param name="Title">Название события</param>
/// <param name="From">Дата, не раньше которой события начинаются</param>
/// <param name="To">Дата, не позже которой события заканчиваются</param>
public record EventsQuery(
    string? Title,
    DateTime? From,
    DateTime? To);
