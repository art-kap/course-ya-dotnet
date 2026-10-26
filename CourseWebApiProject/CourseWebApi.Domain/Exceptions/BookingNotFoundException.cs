namespace CourseWebApi.Domain.Exceptions;

public class BookingNotFoundException(Guid eventId) : EntityNotFoundException("Бронирование", eventId)
{
}
