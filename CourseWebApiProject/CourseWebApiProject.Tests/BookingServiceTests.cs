using CourseWebApiProject.DataAccess;
using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Interfaces;
using CourseWebApiProject.Models;
using CourseWebApiProject.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace CourseWebApiProject.Tests;

public class BookingServiceTests
{
    private readonly ServiceProvider _serviceProvider;

    public BookingServiceTests()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase(dbName));
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IBookingService, BookingService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task CreateBooking_ExistingEvent_Success()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        Guid eventId;
        int availableSeatsBeforeBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            availableSeatsBeforeBooking = eventInfo.AvailableSeats;
            eventId = eventInfo.Id;
        }

        // Act
        BookingInfo booking;
        EventInfo updatedEvent;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

            booking = await bookingService.CreateBookingAsync(eventId);
            updatedEvent = await eventService.GetEvent(eventId);
        }

        // Assert
        booking.Should().NotBeNull();
        booking.EventId.Should().Be(eventId);
        booking.Status.Should().Be((int)BookingStatus.Pending);
        updatedEvent.AvailableSeats.Should().Be(availableSeatsBeforeBooking - 1);
    }

    [Fact]
    public async Task Create_TwoBookings_ShouldCreateDifferentIds()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
        }

        // Act
        BookingInfo firstBooking, secondBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            firstBooking = await bookingService.CreateBookingAsync(eventId);
            secondBooking = await bookingService.CreateBookingAsync(eventId);
        }

        // Assert
        firstBooking.Id.Should().NotBe(secondBooking.Id);
    }

    [Fact]
    public async Task CreateBookings_ExistingEvent_SuccessUntilNoSeatsLeft()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        Guid eventId;
        int availableSeatsBeforeBooking;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
            eventId = eventInfo.Id;
            availableSeatsBeforeBooking = eventInfo.AvailableSeats;
        }

        // Act
        var bookingIdBag = new ConcurrentBag<Guid>();

        for (int i = 0; i < availableSeatsBeforeBooking; i++)
        {
            using var scope = _serviceProvider.CreateScope();
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            var response = await bookingService.CreateBookingAsync(eventId);
            bookingIdBag.Add(response.Id);
        }

        // Assert
        bookingIdBag.Distinct().Count().Should().Be(availableSeatsBeforeBooking);

        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(eventId));
        }
    }

    [Fact]
    public async Task CreateBooking_NonExistingEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
        }

        var nonExistingEventId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(nonExistingEventId));
        }
    }

    [Fact]
    public async Task CreateBooking_RemovedEvent_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        Guid eventId;

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);

            eventId = eventInfo.Id;
            await eventService.RemoveEvent(eventId);
        }

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<EventNotFoundException>(() => bookingService.CreateBookingAsync(eventId));
        }
    }

    [Fact]
    public async Task Get_NonExistingId_ShouldThrowBookingNotFoundException()
    {
        // Arrange
        var validEventDto = EventsTestsHelper.GetValidEventCreate();

        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            var eventInfo = await eventService.AddEvent(validEventDto);
        }

        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
            await Assert.ThrowsAsync<BookingNotFoundException>(() => bookingService.GetBookingByIdAsync(nonExistingId));
        }
    }
}
