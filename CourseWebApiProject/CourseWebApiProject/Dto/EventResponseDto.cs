namespace CourseWebApiProject.Dto;

/// <summary>
/// Полная информация о событии
/// </summary>
/// <param name="Id">id события</param>
/// <param name="Title">Название события</param>
/// <param name="Description">Описание события</param>
/// <param name="StartAt">Точное время начала события</param>
/// <param name="EndAt">Точное время окончания события</param>
/// <param name="TotalSeats">Общее количество мест на событии</param>
/// <param name="AvailableSeats">Текущее количество свободных мест</param>
public record EventResponseDto(
    Guid Id,
    string Title,
    string? Description,
    DateTime StartAt,
    DateTime EndAt,
    int TotalSeats,
    int AvailableSeats);
