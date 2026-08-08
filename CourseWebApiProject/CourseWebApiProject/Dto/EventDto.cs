using System.ComponentModel.DataAnnotations;

namespace CourseWebApiProject.Dto;

/// <summary>
/// Информация о событии
/// </summary>
/// <param name="Id">id события</param>
/// <param name="Title">Название события</param>
/// <param name="Description">Описание события</param>
/// <param name="StartAt">Точное время начала события</param>
/// <param name="EndAt">Точное время окончания события</param>
public record EventDto(
    [Required]
    int Id,
    [Required(ErrorMessage = "Название обязательно для заполнения.")]
    string Title,
    string? Description,
    [Required(ErrorMessage = "Точное время начала обязательно для заполнения.")]
    DateTime StartAt,
    [Required(ErrorMessage = "Точное время окончания обязательно для заполнения.")]
    DateTime EndAt);
