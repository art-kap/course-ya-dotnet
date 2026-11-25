namespace CourseWebApi.Domain.Exceptions;

public class UserNotFoundException(Guid userId) : EntityNotFoundException("Пользователь", userId);
