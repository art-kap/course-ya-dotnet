namespace CourseWebApi.Application.Dto;

/// <summary>
/// Полная информация о пользователе
/// </summary>
/// <param name="Id">id пользователя</param>
/// <param name="Login">Логин</param>
/// <param name="Role">Роль (0 - пользователь, 1 - админ)</param>
public record UserInfo(
    Guid Id,
    string Login,
    int Role);
