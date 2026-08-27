namespace CourseWebApiProject.Exceptions;

public class BookingNotFoundException(Guid eventId) : EntityNotFoundException("Бронирование", eventId)
{
}
