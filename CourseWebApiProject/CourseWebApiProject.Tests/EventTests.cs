using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class EventTests
{
    /// <summary>
    /// Проверяет обновление события
    /// </summary>
    [Fact]
    public async Task Update_Event_UpdatedProperties()
    {
        // Arrange
        var @event = EventsTestsHelper.GetValidEvent();
        var eventId = @event.Id;
        var newTitle = "Updated Event Title";
        var newDescription = "Updated Event Description";
        var newStartAt = @event.StartAt.AddDays(1);
        var newEndAt = @event.EndAt.AddDays(1);

        // Act
        @event.Update(newTitle, newDescription, newStartAt, newEndAt);

        // Assert
        @event.Id.Should().Be(eventId);
        @event.Title.Should().Be(newTitle);
        @event.Description.Should().Be(newDescription);
        @event.StartAt.Should().Be(newStartAt);
        @event.EndAt.Should().Be(newEndAt);
    }

    /// <summary>
    /// Проверяет обновление полей при попытке забронировать место
    /// </summary>
    [Fact]
    public async Task TryReserveSeats_Event_Success()
    {
        // Arrange
        var @event = EventsTestsHelper.GetValidEvent();
        var initialAvailableSeats = @event.AvailableSeats;
        var seatsToReserve = 2;

        // Act
        var canReserve = @event.TryReserveSeats(seatsToReserve);

        // Assert
        canReserve.Should().BeTrue();
        @event.AvailableSeats.Should().Be(initialAvailableSeats - seatsToReserve);
    }

    /// <summary>
    /// Пытается забронировать место до тех пор, пока есть места
    /// </summary>
    [Fact]
    public async Task TryReserveSeats_Event_SuccessUntilNoSeatsAvailable()
    {
        // Arrange
        var @event = EventsTestsHelper.GetValidEvent();
        var totalSeats = @event.TotalSeats;

        // Act
        var firstTry = @event.TryReserveSeats();
        var secondTry = @event.TryReserveSeats(totalSeats);

        // Assert
        firstTry.Should().BeTrue();
        secondTry.Should().BeFalse();
    }

    /// <summary>
    /// Проверяет обновление полей при освобождении места
    /// </summary>
    [Fact]
    public async Task Release_Event_IncreasesAvailableSeats()
    {
        // Arrange
        var @event = EventsTestsHelper.GetValidEvent();
        var initialAvailableSeats = @event.AvailableSeats;
        var seatsToReserve = 2;
        @event.TryReserveSeats(seatsToReserve);

        // Act
        @event.ReleaseSeats(seatsToReserve);

        // Assert
        @event.AvailableSeats.Should().Be(initialAvailableSeats);
    }
}
