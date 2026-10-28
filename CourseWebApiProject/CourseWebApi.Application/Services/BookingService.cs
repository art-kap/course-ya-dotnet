using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Mappings;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;

namespace CourseWebApi.Application.Services;

public class BookingService(IBookingRepository bookingStore, IEventRepository eventStore) : IBookingService
{
    private readonly IBookingRepository _bookingStore = bookingStore;
    private readonly IEventRepository _eventStore = eventStore;
    private readonly static SemaphoreSlim CreateBookingSemaphore = new(1, 1);
    private readonly static TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

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

    public async Task ProcessBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await bookingStore.FindAsync(bookingId, ct) ?? throw new BookingNotFoundException(bookingId);
        var @event = await eventStore.FindAsync(booking.EventId, ct);

        if (@event == null)
        {
            booking.Reject();
            await bookingStore.SaveChangesAsync(ct);

            return;
        }

        // Имитация обработки бронирования
        await Task.Delay(ProcessingDelay, ct);

        booking.Confirm();
        await bookingStore.SaveChangesAsync(ct);
    }
}
