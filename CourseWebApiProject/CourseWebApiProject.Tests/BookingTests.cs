using CourseWebApiProject.Models;
using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class BookingTests
{
    [Fact]
    public async Task Create_Booking_NotNullProperties()
    {
        // Arrange
        var timeNow = DateTime.Now;
        var @event = EventsTestsHelper.GetValidEvent();

        // Act
        var booking = Booking.Create(@event.Id);

        // Assert
        booking.Should().NotBeNull();
        booking.EventId.Should().Be(@event.Id);
        booking.CreatedAt.Should().BeOnOrAfter(timeNow);
        booking.Status.Should().Be(BookingStatus.Pending);
    }

    [Fact]
    public async Task Confirm_Booking_ShouldBeConfirmed()
    {
        // Arrange
        var timeNow = DateTime.Now;

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

    [Fact]
    public async Task Reject_Booking_ShouldBeRejected()
    {
        // Arrange
        var timeNow = DateTime.Now;

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
