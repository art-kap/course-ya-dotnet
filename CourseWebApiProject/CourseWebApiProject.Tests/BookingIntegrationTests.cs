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
using System.Collections.Concurrent;

namespace CourseWebApiProject.Tests;

public class BookingIntegrationTests
{
    private readonly BookingService _bookingService;
    private readonly EventService _eventService;
    private readonly IBookingRepository _bookingRepository;
    private readonly IEventRepository _eventRepository;

    public BookingIntegrationTests()
    {
        _bookingRepository = new InMemoryBookingStore();
        _eventRepository = new InMemoryEventStore();
        _bookingService = new BookingService(_bookingRepository, _eventRepository);
        _eventService = new EventService(_eventRepository);
    }

    [Fact]
    public async Task CreateBooking_ExistingEvent_Success()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventDto();
        var createdEvent = _eventService.AddEvent(validEventDto);
        var availableSeatsBeforeBooking = createdEvent.AvailableSeats;

        // Act
        var response = await _bookingService.CreateBookingAsync(createdEvent.Id);
        var updatedEventDto = _eventService.GetEvent(createdEvent.Id);

        // Assert
        response.Should().NotBeNull();
        response.EventId.Should().Be(createdEvent.Id);
        response.Status.Should().Be((int)BookingStatus.Pending);
        updatedEventDto.AvailableSeats.Should().Be(availableSeatsBeforeBooking - 1);
    }

    [Fact]
    public async Task CreateBookings_ExistingEvent_SuccessUntilNoSeatsLeft()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventDto();
        var createdEvent = _eventService.AddEvent(validEventDto);
        var availableSeatsBeforeBooking = createdEvent.AvailableSeats;

        // Act
        var bookingIdBag = new ConcurrentBag<Guid>();

        for (int i = 0; i < availableSeatsBeforeBooking; i++)
        {
            var response = await _bookingService.CreateBookingAsync(createdEvent.Id);
            bookingIdBag.Add(response.Id);
        };

        // Assert
        bookingIdBag.Count.Should().Be(availableSeatsBeforeBooking);
        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => _bookingService.CreateBookingAsync(createdEvent.Id));
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

    [Fact]
    public async Task ConcurrentBookings_BackgroundService_CorrectProcessingOverbooking()
    {
        // Arrange
        var backgroundService = GetBackgroundService();
        await backgroundService.StartAsync(CancellationToken.None);

        const int totalSeats = 5;
        const int concurrentBookings = 20;

        var validEventDto = EventsTestsHelper.GetValidEventDto(totalSeats);
        var createdEvent = _eventService.AddEvent(validEventDto);

        var successedBookingIdBag = new ConcurrentBag<Guid>();
        var failedBookingsCount = 0;

        // Act
        for (int i = 0; i < concurrentBookings; i++)
        {
            try
            {
                var response = await _bookingService.CreateBookingAsync(createdEvent.Id);
                successedBookingIdBag.Add(response.Id);
            }
            catch (NoAvailableSeatsException)
            {
                failedBookingsCount++;
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(5));
        await backgroundService.StopAsync(CancellationToken.None);

        var confirmedBookingsCount = successedBookingIdBag.Select(async bookingId => await _bookingService.GetBookingByIdAsync(bookingId))
            .Select(task => task.Result)
            .Count(booking => booking.Status == (int)BookingStatus.Confirmed);

        // Assert
        failedBookingsCount.Should().Be(concurrentBookings - totalSeats);
        confirmedBookingsCount.Should().Be(totalSeats);
    }

    [Fact]
    public async Task ConcurrentBookings_BackgroundService_UniqueConfirmedBookings()
    {
        // Arrange
        var backgroundService = GetBackgroundService();
        await backgroundService.StartAsync(CancellationToken.None);

        const int totalSeats = 10;
        const int concurrentBookings = totalSeats;

        var validEventDto = EventsTestsHelper.GetValidEventDto(totalSeats);
        var createdEvent = _eventService.AddEvent(validEventDto);

        var successedBookingIdBag = new ConcurrentBag<Guid>();

        // Act
        for (int i = 0; i < concurrentBookings; i++)
        {
            var response = await _bookingService.CreateBookingAsync(createdEvent.Id);
            successedBookingIdBag.Add(response.Id);
        }

        await Task.Delay(TimeSpan.FromSeconds(5));
        await backgroundService.StopAsync(CancellationToken.None);

        // Assert
        successedBookingIdBag.Count.Should().Be(concurrentBookings);
    }

    private IHostedService GetBackgroundService()
    {
        IServiceCollection services = new ServiceCollection();
        services.AddSingleton(new Mock<ILogger<BookingBackgroundService>>().Object);
        services.AddSingleton(_bookingRepository);
        services.AddSingleton(_eventRepository);
        services.AddHostedService<BookingBackgroundService>();
        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IHostedService>();
    }
}
