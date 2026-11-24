namespace CourseWebApi.Domain.Exceptions;

public class BookingIsAlreadyCancelledException(Guid bookingId) : Exception($"Бронирование {bookingId} было отменено ранее.");
