using CourseWebApi.Domain.Entities;
using CourseWebApi.Domain.Exceptions;
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
        var userId = Guid.NewGuid();

        // Act
        var booking = Booking.Create(@event.Id, userId);

        // Assert
        booking.Should().NotBeNull();
        booking.EventId.Should().Be(@event.Id);
        booking.UserId.Should().Be(userId);
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
        var userId = Guid.NewGuid();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id, userId);

        // Act
        booking.Confirm();

        // Assert
        booking.EventId.Should().Be(@event.Id);
        booking.UserId.Should().Be(userId);
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
        var userId = Guid.NewGuid();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id, userId);

        // Act
        booking.Reject();

        // Assert
        booking.EventId.Should().Be(@event.Id);
        booking.UserId.Should().Be(userId);
        booking.CreatedAt.Should().BeOnOrAfter(timeNow);
        booking.ProcessedAt.Should().BeOnOrAfter(booking.CreatedAt);
        booking.Status.Should().Be(BookingStatus.Rejected);
    }

    /// <summary>
    /// Проверяет изменение статуса бронирования на Cancelled
    /// </summary>
    [Fact]
    public async Task Cancel_Booking_ShouldBeCancelled()
    {
        // Arrange
        var timeNow = DateTime.Now.ToUniversalTime();
        var userId = Guid.NewGuid();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id, userId);

        // Act
        booking.Cancel();

        // Assert
        booking.EventId.Should().Be(@event.Id);
        booking.UserId.Should().Be(userId);
        booking.CreatedAt.Should().BeOnOrAfter(timeNow);
        booking.ProcessedAt.Should().BeOnOrAfter(booking.CreatedAt);
        booking.Status.Should().Be(BookingStatus.Cancelled);
    }

    /// <summary>
    /// Выбрасывает исключение при повторной попытке отменить бронирование
    /// </summary>
    [Fact]
    public async Task Cancel_CancelledBooking_ShouldThrowBookingIsAlreadyCancelledException()
    {
        // Arrange
        var timeNow = DateTime.Now.ToUniversalTime();
        var userId = Guid.NewGuid();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id, userId);

        // Act
        booking.Cancel();

        // Assert
        Assert.Throws<BookingIsAlreadyCancelledException>(() => booking.Cancel());
    }

    /// <summary>
    /// Выбрасывает исключение при отмене отклоненного бронирования
    /// </summary>
    [Fact]
    public async Task Cancel_RejectedBooking_ShouldThrowBookingIsAlreadyRejectedException()
    {
        // Arrange
        var timeNow = DateTime.Now.ToUniversalTime();
        var userId = Guid.NewGuid();

        var @event = EventsTestsHelper.GetValidEvent();
        var booking = Booking.Create(@event.Id, userId);

        // Act
        booking.Reject();

        // Assert
        Assert.Throws<BookingIsAlreadyRejectedException>(() => booking.Cancel());
    }
}
