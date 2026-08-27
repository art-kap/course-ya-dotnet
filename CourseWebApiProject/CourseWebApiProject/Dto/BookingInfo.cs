namespace CourseWebApiProject.Dto;

/// <summary>
/// Информация о брони
/// </summary>
/// <param name="Id">id брони</param>
/// <param name="EventId">id события</param>
/// <param name="Status">Статус брони (0 - Pending, 1 - Confirmed, 2 - Rejected)</param>
/// <param name="CreatedAt">Дата и время создания брони</param>
/// <param name="ProcessedAt">Дата и время обработки брони</param>
public record BookingInfo(
    Guid Id,
    Guid EventId,
    int Status,
    DateTime CreatedAt,
    DateTime? ProcessedAt);
