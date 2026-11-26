namespace CourseWebApi.Application.Dto;

/// <summary>
/// Информация о пользователе
/// </summary>
/// <param name="Id">id пользователя</param>
/// <param name="Login">Логин</param>
public record UserInfo(
    Guid Id,
    string Login);
