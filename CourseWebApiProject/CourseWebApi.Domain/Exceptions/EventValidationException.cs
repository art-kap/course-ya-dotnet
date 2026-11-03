namespace CourseWebApi.Domain.Exceptions;

public class EventValidationException(string message) : Exception($"Ошибка валидации: {message}");
