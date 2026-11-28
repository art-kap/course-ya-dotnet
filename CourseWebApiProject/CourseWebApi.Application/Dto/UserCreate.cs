using CourseWebApi.Domain.Entities;

namespace CourseWebApi.Application.Dto;

/// <summary>
/// Информация для добавления пользователя
/// </summary>
/// <param name="Login">Логин</param>
/// <param name="Password">Пароль</param>
/// <param name="Role">Роль</param>
public record UserCreate(
    string Login,
    string Password,
    UserRole Role = UserRole.User);
