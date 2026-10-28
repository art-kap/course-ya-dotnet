using CourseWebApi.Domain.Entities;
using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class BookingTests
{
    /// <summary>
    /// Проверяет заполнение полей в созданном бронировании
    /// </summary>
    [Fact]
    public async Task Create_Booking_NotNullProperties()
    {
        // Arrange
        var timeNow = DateTime.Now.ToUniversalTime();
        var @event = EventsTestsHelper.GetValidEvent();

        // Act
        var booking = Booking.Create(@event.Id);

        // Assert
        booking.Should().NotBeNull();
        booking.EventId.Should().Be(@event.Id);
        booking.CreatedAt.Should().BeOnOrAfter(timeNow);
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    /// <summary>
    /// Проверяет изменение статуса бронирования на Confirm
    /// </summary>
    [Fact]
    public async Task Confirm_Booking_ShouldBeConfirmed()
    {
        // Arrange
        var timeNow = DateTime.Now.ToUniversalTime();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id);

        // Act
        booking.Confirm();

        // Assert
        booking.EventId.Should().Be(@event.Id);
        booking.CreatedAt.Should().BeOnOrAfter(timeNow);
        booking.ProcessedAt.Should().BeOnOrAfter(booking.CreatedAt);
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    /// <summary>
    /// Проверяет изменение статуса бронирования на Reject
    /// </summary>
    [Fact]
    public async Task Reject_Booking_ShouldBeRejected()
    {
        // Arrange
        var timeNow = DateTime.Now.ToUniversalTime();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id);

        // Act
        booking.Reject();

        // Assert
        booking.EventId.Should().Be(@event.Id);
        booking.CreatedAt.Should().BeOnOrAfter(timeNow);
        booking.ProcessedAt.Should().BeOnOrAfter(booking.CreatedAt);
        booking.Status.Should().Be(BookingStatus.Rejected);
    }
}
