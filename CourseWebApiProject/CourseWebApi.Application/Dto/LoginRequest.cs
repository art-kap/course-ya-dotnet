namespace CourseWebApi.Application.Dto;

/// <summary>
/// Информация для входа пользователя
/// </summary>
/// <param name="Login">Логин</param>
/// <param name="Password">Пароль</param>
public record LoginRequest(
    string Login,
    string Password);
