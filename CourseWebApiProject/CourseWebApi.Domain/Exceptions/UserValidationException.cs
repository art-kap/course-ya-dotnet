namespace CourseWebApi.Domain.Exceptions;

public class UserValidationException(string message) : Exception($"Ошибка валидации: {message}");
