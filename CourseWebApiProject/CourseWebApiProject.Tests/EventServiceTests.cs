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
    private readonly IEventService _eventService;
    private readonly IBookingService _bookingService;

    public EventServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        var serviceProvider = services.BuildServiceProvider();

        _eventService = serviceProvider.GetRequiredService<IEventService>();
        _bookingService = serviceProvider.GetRequiredService<IBookingService>();
    }

    [Fact]
    public async Task Add_ValidEvent_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();

        // Act
        var response = await _eventService.AddEvent(validEvent);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEvent);
    }

    [Fact]
    public async Task Add_EventWithInvalidDates_ShouldThrowEventValidationException()
    {
        // Arrange
        var invalidEvent = EventsTestsHelper.GetEventCreateWithInvalidDates();

        // Act & Assert
        await Assert.ThrowsAsync<EventValidationException>(() => _eventService.AddEvent(invalidEvent));
    }

    [Fact]
    public async Task Add_EventWithInvalidTotalSeats_ShouldThrowEventValidationException()
    {
        // Arrange
        var invalidEventCreate = EventsTestsHelper.GetEventCreateWithInvalidTotalSeats();

        // Act & Assert
        await Assert.ThrowsAsync<EventValidationException>(() => _eventService.AddEvent(invalidEventCreate));
    }

    [Fact]
    public async Task GetEvent_ExistingId_Success()
    {
        // Arrange
        var validEventCreate = EventsTestsHelper.GetValidEventCreate();
        var eventInfo = await _eventService.AddEvent(validEventCreate);

        // Act
        var response = await _eventService.GetEvent(eventInfo.Id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEventCreate);
    }

    [Fact]
    public async Task GetEvent_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        await _eventService.AddEvent(validEvent);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.GetEvent(nonExistingId));
    }

    [Fact]
    public async Task UpdateEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        var eventInfo = await _eventService.AddEvent(validEvent);
        var eventUpdate = EventsTestsHelper.GetValidEventUpdate();

        // Act
        await _eventService.UpdateEvent(eventInfo.Id, eventUpdate);
        var response = await _eventService.GetEvent(eventInfo.Id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(eventUpdate);
    }


    [Fact]
    public async Task UpdateEvent_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        await _eventService.AddEvent(validEvent);
        var eventUpdate = EventsTestsHelper.GetValidEventUpdate();
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.UpdateEvent(nonExistingId, eventUpdate));
    }


    [Fact]
    public async Task UpdateEvent_InvalidDates_ShouldThrowEventValidationException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        var eventInfo = await _eventService.AddEvent(validEvent);
        var invalidEvent = EventsTestsHelper.GetEventUpdateWithInvalidDates();

        // Act & Assert
        await Assert.ThrowsAsync<EventValidationException>(() => _eventService.UpdateEvent(eventInfo.Id, invalidEvent));
    }

    [Fact]
    public async Task RemoveEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        var eventInfo = await _eventService.AddEvent(validEvent);

        // Act
        await _eventService.RemoveEvent(eventInfo.Id);

        // Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.GetEvent(eventInfo.Id));
    }

    [Fact]
    public async Task RemoveEvent_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventCreate();
        await _eventService.AddEvent(validEvent);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _eventService.RemoveEvent(nonExistingId));
    }

    [Fact]
    public async Task RemoveEvent_HasBookings_ShouldThrowEventHasBookingsException()
    {
        // Arrange
        var eventCreate = EventsTestsHelper.GetValidEventCreate();
        var eventInfo = await _eventService.AddEvent(eventCreate);
        await _bookingService.CreateBookingAsync(eventInfo.Id);

        // Act & Assert
        await Assert.ThrowsAsync<EventHasBookingsException>(() => _eventService.RemoveEvent(eventInfo.Id));
    }

    [Fact]
    public async Task GetAll_ThreeEventsOnSinglePage_Success()
    {
        // Arrange
        var events = EventsTestsHelper.GetThreeTestEventCreates(DateTime.Now);
        events.ForEach(async e => await _eventService.AddEvent(e));
        var emptyQuery = new EventsQuery(null, null, null, 1, events.Count);

        // Act
        var paginatedResult = await _eventService.GetEventsByQuery(emptyQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(events.Count);
        paginatedResult.CurrentPageNumber.Should().Be(emptyQuery.Page);
        paginatedResult.CurrentPageSize.Should().Be(events.Count);
    }

    [Fact]
    public async Task GetAll_ThreeEventsOnTwoPages_Success()
    {
        // Arrange
        var events = EventsTestsHelper.GetThreeTestEventCreates(DateTime.Now);
        events.ForEach(async e => await _eventService.AddEvent(e));
        var firstPageQuery = new EventsQuery(null, null, null, 1, 2);
        var secondPageQuery = new EventsQuery(null, null, null, 2, 2);

        // Act
        var firstPageResult = await _eventService.GetEventsByQuery(firstPageQuery);
        var secondPageResult = await _eventService.GetEventsByQuery(secondPageQuery);

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
