using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Services;

public class BookingService(IBookingRepository bookingStore, IEventRepository eventStore) : IBookingService
{
    private readonly IBookingRepository _bookingStore = bookingStore;
    private readonly IEventRepository _eventStore = eventStore;
    private readonly static SemaphoreSlim CreateBookingSemaphore = new(1, 1);

    public async Task<BookingInfo> CreateBookingAsync(Guid eventId)
    {
        Booking bookingToAdd;

        await CreateBookingSemaphore.WaitAsync();

        try
        {
            var @event = await _eventStore.FindAsync(eventId) ?? throw new EventNotFoundException(eventId);
            var canReserve = @event.TryReserveSeats();

            if (!canReserve)
            {
                throw new NoAvailableSeatsException();
            }

            bookingToAdd = Booking.Create(eventId);
            await _bookingStore.Add(bookingToAdd);
        }
        finally
        {
            CreateBookingSemaphore.Release();
        }

        return bookingToAdd.ToInfo();
    }

    public async Task<BookingInfo> GetBookingByIdAsync(Guid bookingId)
    {
        var bookingToGet = await _bookingStore.FindAsync(bookingId) ?? throw new BookingNotFoundException(bookingId);
        return bookingToGet.ToInfo();
    }
}
