using System.ComponentModel.DataAnnotations;

namespace CourseWebApiProject.Dto;

public record EventPutDto(
    [Required(ErrorMessage = "Название обязательно для заполнения.")]
    string Title,
    string? Description,
    [Required(ErrorMessage = "Точное время начала обязательно для заполнения.")]
    DateTime StartAt,
    [Required(ErrorMessage = "Точное время окончания обязательно для заполнения.")]
    DateTime EndAt);
