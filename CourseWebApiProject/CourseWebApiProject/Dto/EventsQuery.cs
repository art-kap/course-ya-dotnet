using System.ComponentModel.DataAnnotations;

namespace CourseWebApiProject.Dto;

/// <summary>
/// Параметры фильтрации событий
/// </summary>
/// <param name="Title">Название события</param>
/// <param name="From">Дата, не раньше которой события начинаются</param>
/// <param name="To">Дата, не позже которой события заканчиваются</param>
/// <param name="Page">Номер страницы</param>
/// <param name="PageSize">Количество элементов на странице</param>
public record EventsQuery(
    string? Title,
    DateTime? From,
    DateTime? To,
    [Range(1, int.MaxValue, ErrorMessage = "Номер страницы должен быть положительным")]
    int Page = 1,
    [Range(1, int.MaxValue, ErrorMessage = "Количество элементов на странице должно быть положительным")]
    int PageSize = 10);
