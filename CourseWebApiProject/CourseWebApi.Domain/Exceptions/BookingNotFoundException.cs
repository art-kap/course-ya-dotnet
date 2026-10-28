namespace CourseWebApi.Domain.Exceptions;

public class BookingNotFoundException(Guid bookingId) : EntityNotFoundException("Бронирование", bookingId);
