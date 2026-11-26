using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Mappings;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;

namespace CourseWebApi.Application.Services;

public class BookingService(IBookingRepository bookingStore, IEventRepository eventStore,
    IUserRepository userStore) : IBookingService
{
    private readonly IBookingRepository _bookingStore = bookingStore;
    private readonly IEventRepository _eventStore = eventStore;
    private readonly IUserRepository _userStore = userStore;
    private readonly static SemaphoreSlim CreateBookingSemaphore = new(1, 1);
    private readonly static TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);
    private readonly static int PendingBookingsUserLimit = 10;

    public async Task<BookingInfo> CreateBookingAsync(Guid eventId, Guid userId)
    {
        Booking bookingToAdd;

        await CreateBookingSemaphore.WaitAsync();

        try
        {
            var bookingsCount = await _bookingStore.CountPendingByUser(userId);

            if (bookingsCount >= PendingBookingsUserLimit)
            {
                throw new PendingBookingsLimitException(PendingBookingsUserLimit);
            }

            var @event = await _eventStore.FindAsync(eventId) ?? throw new EventNotFoundException(eventId);

            if (DateTime.Now.ToUniversalTime() > @event.StartAt)
            {
                throw new PastEventBookingException(@event.Id);
            }

            var canReserve = @event.TryReserveSeats();

            if (!canReserve)
            {
                throw new NoAvailableSeatsException();
            }

            bookingToAdd = Booking.Create(eventId, userId);
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

    public async Task TryConfirmBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingStore.FindAsync(bookingId, ct) ?? throw new BookingNotFoundException(bookingId);
        var @event = await _eventStore.FindAsync(booking.EventId, ct) ?? throw new EventNotFoundException(booking.EventId);

        // Имитация обработки бронирования
        await Task.Delay(ProcessingDelay, ct);

        booking.Confirm();
        await _bookingStore.SaveChangesAsync(ct);
    }

    public async Task RejectBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var booking = await _bookingStore.FindAsync(bookingId, ct) ?? throw new BookingNotFoundException(bookingId);
        var @event = await _eventStore.FindAsync(booking.EventId, ct) ?? throw new EventNotFoundException(booking.EventId);

        booking.Reject();
        @event.ReleaseSeats();
        await _bookingStore.SaveChangesAsync(ct);
    }

    public async Task CancelBooking(Guid bookingId, Guid userId)
    {
        var booking = await _bookingStore.FindAsync(bookingId) ?? throw new BookingNotFoundException(bookingId);
        var user = await _userStore.FindById(userId) ?? throw new UserNotFoundException(userId);

        if (booking.UserId != userId && user.Role != UserRole.Admin)
        {
            throw new AccessDeniedException(userId);
        }

        booking.Cancel();
        await _bookingStore.SaveChangesAsync();
    }
}
