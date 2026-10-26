using CourseWebApi.Domain.Entities;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Repository;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.IntegrationTests;

[Collection("Database collection")]
public class EventRepositoryTests(PostgreSqlFixture fixture) : BaseRepositoryTests(fixture)
{
    [Fact]
    public async Task Add_Event_SavesEventToDatabase()
    {
        // Arrange
        var @event = GetTestEvent();
        var eventId = @event.Id;

        // Act
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Assert
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var hasEvent = await eventRepository.ContainsId(eventId);
            hasEvent.Should().BeTrue();
        }
    }

    [Fact]
    public async Task FindEvent_ExistingId_EventFound()
    {
        // Arrange
        var eventCreate = GetTestEventCreate();
        Guid eventId;
        int availableSeats;

        await using (var context = CreateContext())
        {
            var @event = eventCreate.ToEvent();
            eventId = @event.Id;
            availableSeats = @event.AvailableSeats;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act
        Event? savedEvent;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            savedEvent = await eventRepository.FindAsync(eventId);
        }

        // Assert
        savedEvent.Should().NotBeNull();
        savedEvent.Title.Should().Be(eventCreate.Title);
        savedEvent.Description.Should().Be(eventCreate.Description);
        savedEvent.StartAt.Should().BeCloseTo(eventCreate.StartAt!.Value, 1.Microseconds());
        savedEvent.EndAt.Should().BeCloseTo(eventCreate.EndAt!.Value, 1.Microseconds());
        savedEvent.TotalSeats.Should().Be(eventCreate.TotalSeats);
        savedEvent.AvailableSeats.Should().Be(availableSeats);
    }

    [Fact]
    public async Task Find_NonExistingId_EventNotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var nullEvent = await eventRepository.FindAsync(nonExistingId);
            nullEvent.Should().BeNull();
        }
    }

    [Fact]
    public async Task Add_DuplicateEvent_ThrowsDbUpdateException()
    {
        // Arrange
        var @event = GetTestEvent();

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            await Assert.ThrowsAsync<DbUpdateException>(() => eventRepository.Add(@event));
        }
    }

    [Fact]
    public async Task Update_ExistingEvent_SaveChangesToDatabase()
    {
        // Arrange
        Guid eventId;
        int totalSeats, availableSeats;

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            eventId = @event.Id;
            totalSeats = @event.TotalSeats;
            availableSeats = @event.AvailableSeats;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        var eventUpdate = GetTestEventUpdate();

        // Act
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var @event = await eventRepository.FindAsync(eventId);
            @event!.Update(eventUpdate.Title, eventUpdate.Description,
                eventUpdate.StartAt!.Value, eventUpdate.EndAt!.Value);
            await eventRepository.SaveChangesAsync();
        }

        // Assert
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var updatedEvent = await eventRepository.FindAsync(eventId);

            updatedEvent.Should().NotBeNull();
            updatedEvent.Title.Should().Be(eventUpdate.Title);
            updatedEvent.Description.Should().Be(eventUpdate.Description);
            updatedEvent.StartAt.Should().BeCloseTo(eventUpdate.StartAt!.Value, 1.Microseconds());
            updatedEvent.EndAt.Should().BeCloseTo(eventUpdate.EndAt!.Value, 1.Microseconds());
            updatedEvent.TotalSeats.Should().Be(totalSeats);
            updatedEvent.AvailableSeats.Should().Be(availableSeats);
        }
    }

    [Fact]
    public async Task Remove_ExistingId_ShouldReturnTrue()
    {
        // Arrange
        var @event = GetTestEvent();
        var eventId = @event.Id;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act
        bool okRemoved;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            okRemoved = await eventRepository.RemoveById(eventId);
        }

        // Assert
        await using (var context = CreateContext())
        {
            okRemoved.Should().BeTrue();

            var eventRepository = new EventRepository(context);
            var hasEvent = await eventRepository.ContainsId(eventId);
            hasEvent.Should().BeFalse();
        }
    }

    [Fact]
    public async Task Remove_NonExistingId_ShouldReturnFalse()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);
        }

        // Act
        bool okRemoved;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            okRemoved = await eventRepository.RemoveById(nonExistingId);
        }

        // Assert
        okRemoved.Should().BeFalse();
    }

    [Fact]
    public async Task Remove_EventWithBooking_ShouldThrowsDbUpdateException()
    {
        // Arrange
        Guid eventId;

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            eventId = @event.Id;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);

            var booking = Booking.Create(eventId);

            var bookingRepository = new BookingRepository(context);
            await bookingRepository.Add(booking);
        }

        // Act & Assert
        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            await Assert.ThrowsAsync<DbUpdateException>(() => eventRepository.RemoveById(eventId));
        }
    }

    [Fact]
    public async Task GetAll_Events_EveryEventExists()
    {
        // Arrange
        var eventsCount = 10;
        var eventIds = new List<Guid>();

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);

            foreach (var _ in Enumerable.Range(0, eventsCount))
            {
                var @event = GetTestEvent();
                eventIds.Add(@event.Id);
                await eventRepository.Add(@event);
            }
        }

        // Act
        IEnumerable<Event> allEvents;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            allEvents = await eventRepository.GetAll();
        }

        // Assert
        allEvents.Count().Should().Be(eventsCount);

        foreach (var eventId in eventIds)
        {
            allEvents.Any(e => e.Id == eventId).Should().BeTrue();
        }
    }

    [Fact]
    public async Task Load_Include_ReturnsEventWithBookings()
    {
        // Arrange
        Guid eventId;
        var expectedBookingIds = new List<string>();

        await using (var context = CreateContext())
        {
            var @event = GetTestEvent();
            eventId = @event.Id;

            var eventRepository = new EventRepository(context);
            await eventRepository.Add(@event);

            var bookingRepository = new BookingRepository(context);

            foreach (var _ in Enumerable.Range(0, @event.AvailableSeats))
            {
                var booking = Booking.Create(eventId);
                await bookingRepository.Add(booking);
                expectedBookingIds.Add(booking.Id.ToString());
            }
        }

        // Act
        Event loadedEvent;

        await using (var context = CreateContext())
        {
            loadedEvent = await context.Events.Include(b => b.Bookings).FirstAsync(e => e.Id == eventId);
        }

        var loadedBookingIds = loadedEvent.Bookings.Select(b => b.Id.ToString());

        // Assert
        loadedEvent.Bookings.Should().NotBeNull();
        expectedBookingIds.Should().AllSatisfy(b => b.Should().BeOneOf(loadedBookingIds));
    }
}
