using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using CourseWebApiProject.Repository;
using CourseWebApiProject.Services;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace CourseWebApiProject.Tests;

public class BookingIntegrationTests
{
    private readonly BookingService _bookingService;
    private readonly EventService _eventService;
    private readonly IBookingRepository _bookingRepository;

    public BookingIntegrationTests()
    {
        var eventRepository = new InMemoryEventStore();
        _bookingRepository = new InMemoryBookingStore();
        _bookingService = new BookingService(_bookingRepository, eventRepository);
        _eventService = new EventService(eventRepository);
    }

    [Fact]
    public async Task CreateBooking_ExistingEvent_Success()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventDto();
        var createdEvent = _eventService.AddEvent(validEventDto);

        // Act
        var response = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Assert
        response.Should().NotBeNull();
        response.EventId.Should().Be(createdEvent.Id);
        response.Status.Should().Be((int)BookingStatus.Pending);
    }

    [Fact]
    public async Task CreateBooking_NonExistingEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventDto();
        var createdEvent = _eventService.AddEvent(validEventDto);
        var nonExistingEventId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(nonExistingEventId));
    }

    [Fact]
    public async Task CreateBooking_RemovedEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventDto();
        var eventToRemove = _eventService.AddEvent(validEventDto);
        var eventId = eventToRemove.Id;
        _eventService.RemoveEvent(eventId);

        // Act & Assert
        await Assert.ThrowsAsync<EventNotFoundException>(() => _bookingService.CreateBookingAsync(eventId));
    }

    [Fact]
    public async Task Wait_BackgroundConfirmation_ShouldConfirmBooking()
    {
        // Arrange
        var backgroundService = GetBackgroundService();
        await backgroundService.StartAsync(CancellationToken.None);

        var validEventDto = EventsTestsHelper.GetValidEventDto();
        var createdEvent = _eventService.AddEvent(validEventDto);
        var booking = await _bookingService.CreateBookingAsync(createdEvent.Id);
        var bookingId = booking.Id;

        // Act
        await Task.Delay(TimeSpan.FromSeconds(5));
        await backgroundService.StopAsync(CancellationToken.None);
        var handledBooking = await _bookingService.GetBookingByIdAsync(bookingId);

        // Assert
        handledBooking.Id.Should().Be(bookingId);
        handledBooking.EventId.Should().Be(createdEvent.Id);
        handledBooking.Status.Should().Be((int)BookingStatus.Confirmed);
    }

    private IHostedService GetBackgroundService()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Mock<ILogger<BookingBackgroundService>>().Object);
        services.AddSingleton(_bookingRepository);
        services.AddHostedService<BookingBackgroundService>();
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IHostedService>();
    }
}
