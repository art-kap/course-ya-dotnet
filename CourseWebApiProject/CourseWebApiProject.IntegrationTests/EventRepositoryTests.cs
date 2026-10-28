using CourseWebApi.Application.Mappings;
using CourseWebApi.Domain.Entities;
using CourseWebApi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CourseWebApiProject.IntegrationTests;

[Collection("Database collection")]
public class EventRepositoryTests(PostgreSqlFixture fixture) : BaseRepositoryTests(fixture)
{
    /// <summary>
    /// Проверяет добавление события в БД
    /// </summary>
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

    /// <summary>
    /// Проверяет поиск события по существующему id
    /// </summary>
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

    /// <summary>
    /// Проверяет поиск события по несуществующему id
    /// </summary>
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

    /// <summary>
    /// Неудача при попытке добавить два события с одинаковыми id
    /// </summary>
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

    /// <summary>
    /// Проверяет обновление события в БД
    /// </summary>
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

    /// <summary>
    /// Проверяет удаление существующего события
    /// </summary>
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

    /// <summary>
    /// Проверяет удаление несуществующего события
    /// </summary>
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

    /// <summary>
    /// Проверяет удаление события, у которого есть бронирования
    /// </summary>
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

    /// <summary>
    /// Проверяет получение всех событий
    /// </summary>
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

    /// <summary>
    /// Проверяет получение событий вместе с бронированиями
    /// </summary>
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

    /// <summary>
    /// Проверяет фильтрацию событий по названию
    /// </summary>
    [Theory]
    [InlineData("test event")]
    [InlineData("TeSt evEnT")]
    [InlineData("TEsT")]
    [InlineData("EST eve")]
    [InlineData("st evenT")]
    public async Task FilterByTitle_RandomChoiceFromTwoTitles_ShouldReturnUnchangedEvents(string title)
    {
        // Arrange
        int totalEventsCount = 20;
        int maxPageSize = 5;
        int maxPageNumber = 5;

        var pageSize = new Random().Next(1, maxPageSize);
        var pageNumber = new Random().Next(1, maxPageNumber);

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);

            foreach (var _ in Enumerable.Range(0, totalEventsCount))
            {
                var @event = GetTestEvent();
                await eventRepository.Add(@event);
            }
        }

        var updateEventsCount = new Random().Next(totalEventsCount);

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var allEvents = await eventRepository.GetAll();
            var eventUpdate = GetTestEventUpdate();

            int counter = 0;

            foreach (var @event in allEvents)
            {
                if (counter == updateEventsCount)
                {
                    break;
                }

                @event.Update(eventUpdate.Title, eventUpdate.Description, eventUpdate.StartAt!.Value, eventUpdate.EndAt!.Value);
                counter++;
            }

            await eventRepository.SaveChangesAsync();
        }

        // Act
        EventsPage? eventsPage;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            eventsPage = await eventRepository.GetFiltered(new EventsFilter(title, null, null, pageNumber, pageSize));
        }

        // Assert
        eventsPage.Should().NotBeNull();
        eventsPage.EventsCount.Should().Be(totalEventsCount - updateEventsCount);
        eventsPage.CurrentPageNumber.Should().Be(pageNumber);
        eventsPage.CurrentPageSize.Should().BeLessThanOrEqualTo(pageSize);
    }

    /// <summary>
    /// Проверяет фильтрацию событий по дате
    /// </summary>
    [Fact]
    public async Task FilterByDate_RandomChoiceFromTwoStartDates_ShouldReturnUnchangedEvents()
    {
        // Arrange
        int totalEventsCount = 20;
        int maxPageSize = 5;
        int maxPageNumber = 5;

        var pageSize = new Random().Next(1, maxPageSize);
        var pageNumber = new Random().Next(1, maxPageNumber);

        var startAt = new DateTime(_timeNow.Year, 1, 1).ToUniversalTime();
        var endAt = startAt.AddYears(1).ToUniversalTime();

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);

            foreach (var _ in Enumerable.Range(0, totalEventsCount))
            {
                var @event = GetTestEvent();
                await eventRepository.Add(@event);
            }
        }

        var updateEventsCount = new Random().Next(totalEventsCount);

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var allEvents = await eventRepository.GetAll();
            var eventUpdate = GetTestEventUpdate();

            int counter = 0;

            foreach (var @event in allEvents)
            {
                if (counter == updateEventsCount)
                {
                    break;
                }

                @event.Update(eventUpdate.Title, eventUpdate.Description, eventUpdate.StartAt!.Value, eventUpdate.EndAt!.Value);
                counter++;
            }

            await eventRepository.SaveChangesAsync();
        }

        // Act
        EventsPage? eventsPage;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            eventsPage = await eventRepository.GetFiltered(new EventsFilter(null, startAt, endAt, pageNumber, pageSize));
        }

        // Assert
        eventsPage.Should().NotBeNull();
        eventsPage.EventsCount.Should().Be(totalEventsCount - updateEventsCount);
        eventsPage.CurrentPageNumber.Should().Be(pageNumber);
        eventsPage.CurrentPageSize.Should().BeLessThanOrEqualTo(pageSize);
    }

    /// <summary>
    /// Проверяет фильтрацию событий по названию и дате
    /// </summary>
    [Theory]
    [InlineData("test event")]
    [InlineData("TeSt evEnT")]
    [InlineData("TEsT")]
    [InlineData("EST eve")]
    [InlineData("st evenT")]
    public async Task FilterByTitleAndDates_RandomChoice_ShouldReturnUnchangedEvents(string title)
    {
        // Arrange
        int totalEventsCount = 20;
        int maxPageSize = 5;
        int maxPageNumber = 5;
        var changeProbabilityThreshold = 0.5;

        var pageSize = new Random().Next(1, maxPageSize);
        var pageNumber = new Random().Next(1, maxPageNumber);

        var startAt = new DateTime(_timeNow.Year, 1, 1).ToUniversalTime();
        var endAt = startAt.AddYears(1).ToUniversalTime();

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);

            foreach (var _ in Enumerable.Range(0, totalEventsCount))
            {
                var @event = GetTestEvent();
                await eventRepository.Add(@event);
            }
        }

        var updateEventsCount = 0;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            var allEvents = await eventRepository.GetAll();
            var eventUpdate = GetTestEventUpdate();

            foreach (var @event in allEvents)
            {
                // обновляем или только название, или только даты, или всё вместе
                var needUpdateTitle = new Random().NextDouble() < changeProbabilityThreshold;
                var needUpdateDates = new Random().NextDouble() < changeProbabilityThreshold;

                var newTitle = needUpdateTitle ? eventUpdate.Title : @event.Title;
                var newStartAt = needUpdateDates ? eventUpdate.StartAt!.Value : @event.StartAt;
                var newEndAt = needUpdateDates ? eventUpdate.EndAt!.Value : @event.EndAt;

                @event.Update(newTitle, eventUpdate.Description, newStartAt, newEndAt);

                if (needUpdateTitle || needUpdateDates)
                {
                    updateEventsCount++;
                }
            }

            await eventRepository.SaveChangesAsync();
        }

        // Act
        EventsPage? eventsPage;

        await using (var context = CreateContext())
        {
            var eventRepository = new EventRepository(context);
            eventsPage = await eventRepository.GetFiltered(new EventsFilter(title, startAt, endAt, pageNumber, pageSize));
        }

        // Assert
        eventsPage.Should().NotBeNull();
        eventsPage.EventsCount.Should().Be(totalEventsCount - updateEventsCount);
        eventsPage.CurrentPageNumber.Should().Be(pageNumber);
        eventsPage.CurrentPageSize.Should().BeLessThanOrEqualTo(pageSize);
    }

}
