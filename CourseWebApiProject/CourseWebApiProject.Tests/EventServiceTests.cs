using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Interfaces;
using CourseWebApi.Application.Services;
using CourseWebApi.Domain.Exceptions;
using CourseWebApi.Infrastructure.Persistence;
using CourseWebApi.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CourseWebApiProject.Tests;

public class EventServiceTests
{
    private readonly ServiceProvider _serviceProvider;

    public EventServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    /// <summary>
    /// Успешное добавление события
    /// </summary>
    [Fact]
    public async Task Add_ValidEvent_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();

        // Act
        EventInfo response;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            response = await eventService.AddEvent(validEvent);
        }

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEvent);
    }

    /// <summary>
    /// Неудача при добавлении события с неправильными датами
    /// </summary>
    [Fact]
    public async Task Add_EventWithInvalidDates_ShouldThrowEventValidationException()
    {
        // Arrange
        var invalidEvent = EventsTestsHelper.GetEventCreateWithInvalidDates();

        // Act & Assert
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        await Assert.ThrowsAsync<EventValidationException>(() => eventService.AddEvent(invalidEvent));
    }

    /// <summary>
    /// Неудача при добавлении события с неправильным количеством мест
    /// </summary>
    [Fact]
    public async Task Add_EventWithInvalidTotalSeats_ShouldThrowEventValidationException()
    {
        // Arrange
        var invalidEventCreate = EventsTestsHelper.GetEventCreateWithInvalidTotalSeats();

        // Act & Assert
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        await Assert.ThrowsAsync<EventValidationException>(() => eventService.AddEvent(invalidEventCreate));
    }

    /// <summary>
    /// Успешное получение события по id
    /// </summary>
    [Fact]
    public async Task GetEvent_ExistingId_Success()
    {
        // Arrange
        var validEventCreate = EventsTestsHelper.GetValidEventCreate();
        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventCreate);
            eventId = eventInfo.Id;
        }

        // Act
        EventInfo response;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            response = await eventService.GetEvent(eventId);
        }

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEventCreate);
    }

    /// <summary>
    /// Неудача при попытке получить событие по несуществущему id
    /// </summary>
    [Fact]
    public async Task GetEvent_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await eventService.AddEvent(validEvent);
        }

        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => eventService.GetEvent(nonExistingId));
        }
    }

    /// <summary>
    /// Успешное обновление события
    /// </summary>
    [Fact]
    public async Task UpdateEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        var eventUpdate = EventsTestsHelper.GetValidEventUpdate();
        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEvent);
            eventId = eventInfo.Id;
        }

        // Act
        EventInfo response;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await eventService.UpdateEvent(eventId, eventUpdate);
            response = await eventService.GetEvent(eventId);
        }

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(eventUpdate);
    }

    /// <summary>
    /// Неудача при попытке обновить событие по несуществущему id
    /// </summary>
    [Fact]
    public async Task UpdateEvent_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        var eventUpdate = EventsTestsHelper.GetValidEventUpdate();

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await eventService.AddEvent(validEvent);
        }

        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => eventService.UpdateEvent(nonExistingId, eventUpdate));
        }
    }

    /// <summary>
    /// Неудача при попытке получить событие некорректными данными
    /// </summary>
    [Fact]
    public async Task UpdateEvent_InvalidDates_ShouldThrowEventValidationException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        var invalidEvent = EventsTestsHelper.GetEventUpdateWithInvalidDates();
        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEvent);
            eventId = eventInfo.Id;
        }

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<EventValidationException>(() => eventService.UpdateEvent(eventId, invalidEvent));
        }
    }

    /// <summary>
    /// Успешное удаление события
    /// </summary>
    [Fact]
    public async Task RemoveEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEvent);
            eventId = eventInfo.Id;
        }

        // Act
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await eventService.RemoveEvent(eventId);
        }

        // Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => eventService.GetEvent(eventId));
        }
    }

    /// <summary>
    /// Неудача при попытке удалить событие по несуществущему id
    /// </summary>
    [Fact]
    public async Task RemoveEvent_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await eventService.AddEvent(validEvent);
        }

        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => eventService.RemoveEvent(nonExistingId));
        }
    }

    /// <summary>
    /// Неудача при попытке удалить событие с имеющимися бронированиями
    /// </summary>
    [Fact]
    public async Task RemoveEvent_HasBookings_ShouldThrowEventHasBookingsException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEvent);
            eventId = eventInfo.Id;

            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await bookingService.CreateBookingAsync(eventId);
        }

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            await Assert.ThrowsAsync<EventHasBookingsException>(() => eventService.RemoveEvent(eventId));
        }
    }

    /// <summary>
    /// Успешное получение всех событий на одной странице
    /// </summary>
    [Fact]
    public async Task GetAll_ThreeEventsOnSinglePage_Success()
    {
        // Arrange
        var events = EventsTestsHelper.GetThreeTestEventCreates(DateTime.Now);

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            events.ForEach(async e => await eventService.AddEvent(e));
        }

        var emptyQuery = new EventsQuery(null, null, null, 1, events.Count);

        // Act
        PaginatedResult? paginatedResult;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            paginatedResult = await eventService.GetEventsByQuery(emptyQuery);
        }

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(events.Count);
        paginatedResult.CurrentPageNumber.Should().Be(emptyQuery.Page);
        paginatedResult.CurrentPageSize.Should().Be(events.Count);
    }

    /// <summary>
    /// Успешное получение событий на двух страницах
    /// </summary>
    [Fact]
    public async Task GetAll_ThreeEventsOnTwoPages_Success()
    {
        // Arrange
        var events = EventsTestsHelper.GetThreeTestEventCreates(DateTime.Now);

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            events.ForEach(async e => await eventService.AddEvent(e));
        }

        var firstPageQuery = new EventsQuery(null, null, null, 1, 2);
        var secondPageQuery = new EventsQuery(null, null, null, 2, 2);

        // Act
        PaginatedResult? firstPageResult, secondPageResult;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            firstPageResult = await eventService.GetEventsByQuery(firstPageQuery);
            secondPageResult = await eventService.GetEventsByQuery(secondPageQuery);
        }

        // Assert
        firstPageResult.Should().NotBeNull();
        firstPageResult.EventsCount.Should().Be(events.Count);
        firstPageResult.CurrentPageNumber.Should().Be(firstPageQuery.Page);
        firstPageResult.CurrentPageSize.Should().Be(2);

        secondPageResult.Should().NotBeNull();
        secondPageResult.EventsCount.Should().Be(events.Count);
        secondPageResult.CurrentPageNumber.Should().Be(secondPageQuery.Page);
        secondPageResult.CurrentPageSize.Should().Be(1);
    }
}
