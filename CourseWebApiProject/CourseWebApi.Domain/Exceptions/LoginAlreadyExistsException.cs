namespace CourseWebApi.Domain.Exceptions;

public class LoginAlreadyExistsException(string login) : Exception($"Пользователь с логином {login} уже существует");
