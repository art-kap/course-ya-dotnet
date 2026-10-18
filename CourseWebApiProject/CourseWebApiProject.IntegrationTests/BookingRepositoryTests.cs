using CourseWebApiProject.Models;
using CourseWebApiProject.Repository;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.IntegrationTests;

[Collection("Database collection")]
public class BookingRepositoryTests(PostgreSqlFixture fixture) : BaseRepositoryTests(fixture)
{
    [Fact]
    public async Task FindBooking_ExistingId_BookingFound()
    {
        // Arrange
        Guid eventId, bookingId;
        Booking booking;
        var timeNow = DateTime.Now.ToUniversalTime();

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            eventId = @event.Id;

            booking = Booking.Create(eventId);
            bookingId = booking.Id;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act
        await using (var context = CreateContext())
        {
            var bookingRepository = new BookingRepository(context);
            await bookingRepository.Add(booking);
        }

        // Assert
        await using (var context = CreateContext())
        {
            var bookingRepository = new BookingRepository(context);
            var savedBooking = await bookingRepository.FindAsync(bookingId);

            savedBooking.Should().NotBeNull();
            savedBooking.EventId.Should().Be(eventId);
            savedBooking.CreatedAt.Should().BeOnOrAfter(timeNow);
            savedBooking.Status.Should().Be(BookingStatus.Pending);
        }
    }

    [Fact]
    public async Task FindBooking_NonExistingId_BookingNotFound()
    {
        // Arrange
        Guid nonExistingId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            var booking = Booking.Create(@event.Id);
            var bookingId = booking.Id;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);

            var bookingRepository = new BookingRepository(context);
            await bookingRepository.Add(booking);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var bookingRepository = new BookingRepository(context);
            var nullBooking = await bookingRepository.FindAsync(nonExistingId);
            nullBooking.Should().BeNull();
        }
    }

    [Fact]
    public async Task Add_DuplicateBooking_ThrowsDbUpdateException()
    {
        // Arrange
        var @event = GetTestEvent();
        var booking = Booking.Create(@event.Id);

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);

            var bookingRepository = new BookingRepository(context);
            await bookingRepository.Add(booking);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var bookingRepository = new BookingRepository(context);
            await Assert.ThrowsAsync<DbUpdateException>(() => bookingRepository.Add(booking));
        }
    }

    [Fact]
    public async Task AddBooking_NonExistingEvent_ThrowsDbUpdateException()
    {
        // Arrange
        var nonExistingEventId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var booking = Booking.Create(nonExistingEventId);
            var bookingRepository = new BookingRepository(context);
            await Assert.ThrowsAsync<DbUpdateException>(() => bookingRepository.Add(booking));
        }
    }

    [Fact]
    public async Task GetPending_RandomChoiceForPendingBooking_ReturnsPendingBooking()
    {
        // Arrange
        Guid pendingBookingId = default;

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);

            var bookingRepository = new BookingRepository(context);

            foreach (var _ in Enumerable.Range(0, @event.AvailableSeats))
            {
                await bookingRepository.Add(Booking.Create(@event.Id));
            }

            var bookings = await bookingRepository.GetAll();
            var indexOfPendingBooking = new Random().Next(bookings.Count - 1);
            pendingBookingId = bookings.ElementAtOrDefault(indexOfPendingBooking)!.Id;

            foreach (var booking in bookings)
            {
                if (booking.Id != pendingBookingId)
                {
                    booking.Confirm();
                }
            }

            await bookingRepository.SaveChangesAsync();
        }

        // Act
        IEnumerable<Booking> pendingBookings;

        await using (var context = CreateContext())
        {
            var bookingRepository = new BookingRepository(context);
            pendingBookings = await bookingRepository.GetPending();
        }

        // Assert
        pendingBookings.Count().Should().Be(1);
        pendingBookings.First().Id.Should().Be(pendingBookingId);
    }

    [Fact]
    public async Task Load_Include_ReturnsBookingWithEvent()
    {
        // Arrange
        Guid eventId, bookingId;

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            eventId = @event.Id;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);

            var booking = Booking.Create(eventId);
            bookingId = booking.Id;

            var bookingRepository = new BookingRepository(context);
            await bookingRepository.Add(booking);
        }

        // Act
        Booking loadedBooking;

        await using (var context = CreateContext())
        {
            loadedBooking = await context.Bookings.Include(b => b.Event).FirstAsync(b => b.Id == bookingId);
        }

        // Assert
        loadedBooking.Event.Should().NotBeNull();
        loadedBooking.Event.Id.Should().Be(eventId);
    }
}
