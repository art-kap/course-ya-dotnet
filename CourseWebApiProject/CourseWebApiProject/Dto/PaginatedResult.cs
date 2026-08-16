namespace CourseWebApiProject.Dto;

/// <summary>
/// Результат пагинации
/// </summary>
/// <param name="EventsCount">Общее количество событий</param>
/// <param name="CurrentPageEvents">События на текущей странице</param>
/// <param name="CurrentPageNumber">Номер текущей страницы</param>
/// <param name="CurrentPageSize">Количество элементов на текущей странице</param>
public record PaginatedResult(
    int EventsCount,
    EventResponseDto[] CurrentPageEvents,
    int CurrentPageNumber,
    int CurrentPageSize);
