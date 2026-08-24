using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Repository;
using CourseWebApiProject.Services;
using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class EventServiceTests
{
    private readonly EventService _eventService;

    private readonly DateTime _startAt;
    private readonly DateTime _endAtValid;
    private readonly DateTime _endAtInvalid;

    private readonly string _previousMonthTitle = "event in the previous month";
    private readonly string _currentMonthTitle = "event in the current month";
    private readonly string _nextMonthTitle = "event in the next month";

    public EventServiceTests()
    {
        _eventService = new EventService(new InMemoryEventStore());

        _startAt = DateTime.Now;
        _endAtValid = _startAt.AddHours(1);
        _endAtInvalid = _startAt.AddHours(-1);
    }

    [Fact]
    public void Add_ValidEvent_Success()
    {
        // Arrange
        var validEvent = GetValidEvent();

        // Act
        var response = _eventService.AddEvent(validEvent);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEvent);
    }

    [Fact]
    public void Add_EventWithInvalidDates_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidEvent = GetEventWithInvalidDates();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _eventService.AddEvent(invalidEvent));
    }

    [Fact]
    public void Get_ExistingId_Success()
    {
        // Arrange
        var validEvent = GetValidEvent();
        var id = _eventService.AddEvent(validEvent).Id;

        // Act
        var response = _eventService.GetEvent(id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEvent);
    }

    [Fact]
    public void Get_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = GetValidEvent();
        _eventService.AddEvent(validEvent);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.GetEvent(nonExistingId));
    }

    [Fact]
    public void Update_ExistingId_Success()
    {
        // Arrange
        var validEvent = GetValidEvent();
        var id = _eventService.AddEvent(validEvent).Id;
        var anotherValidEvent = GetAnotherValidEvent();

        // Act
        _eventService.UpdateEvent(id, anotherValidEvent);
        var response = _eventService.GetEvent(id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(anotherValidEvent);
    }

    [Fact]
    public void Update_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = GetValidEvent();
        _eventService.AddEvent(validEvent);
        var anotherValidEvent = GetAnotherValidEvent();
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.UpdateEvent(nonExistingId, anotherValidEvent));
    }

    [Fact]
    public void Update_InvalidDates_ShouldThrowArgumentException()
    {
        // Arrange
        var validEvent = GetValidEvent();
        var id = _eventService.AddEvent(validEvent).Id;
        var invalidEvent = GetEventWithInvalidDates();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _eventService.UpdateEvent(id, invalidEvent));
    }

    [Fact]
    public void Remove_ExistingId_Success()
    {
        // Arrange
        var validEvent = GetValidEvent();
        var id = _eventService.AddEvent(validEvent).Id;

        // Act
        _eventService.RemoveEvent(id);

        // Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.GetEvent(id));
    }

    [Fact]
    public void Remove_NonExistingId_ShouldThrowEventNotFoundException()
    {
        // Arrange
        var validEvent = GetValidEvent();
        _eventService.AddEvent(validEvent);
        var nonExistingId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.RemoveEvent(nonExistingId));
    }

    [Fact]
    public void GetAll_ThreeEventsOnSinglePage_Success()
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var emptyQuery = new EventsQuery(null, null, null, 1, events.Count);

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(emptyQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(events.Count);
        paginatedResult.CurrentPageNumber.Should().Be(emptyQuery.Page);
        paginatedResult.CurrentPageSize.Should().Be(events.Count);
    }

    [Fact]
    public void GetAll_ThreeEventsOnTwoPages_Success()
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var firstPageQuery = new EventsQuery(null, null, null, 1, 2);
        var secondPageQuery = new EventsQuery(null, null, null, 2, 2);

        // Act
        var firstPageResult = _eventService.GetEventsByQuery(firstPageQuery);
        var secondPageResult = _eventService.GetEventsByQuery(secondPageQuery);

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

    [Fact]
    public void Filter_Title_Success()
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery("NEXT", null, null);

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(1);
        paginatedResult.CurrentPageEvents.First().Title.Should().Be(_nextMonthTitle);
    }

    [Fact]
    public void Filter_DateFrom_Success()
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(null, _startAt, null);
        var expectedTitles = new string[] { _currentMonthTitle, _nextMonthTitle };

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(2);
        paginatedResult.CurrentPageEvents.Should().AllSatisfy(e => e.Title.Should().BeOneOf(expectedTitles));
    }

    [Fact]
    public void Filter_DateTo_Success()
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(null, null, _endAtValid);
        var expectedTitles = new string[] { _previousMonthTitle, _currentMonthTitle };

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(2);
        paginatedResult.CurrentPageEvents.Should().AllSatisfy(e => e.Title.Should().BeOneOf(expectedTitles));
    }

    [Fact]
    public void Filter_DatesFromTo_Success()
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(null, _startAt, _endAtValid);

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(1);
        paginatedResult.CurrentPageEvents.First().Title.Should().Be(_currentMonthTitle);
    }

    [Theory]
    [InlineData("Current")]
    [InlineData("cuRRent")]
    [InlineData("event", 1)]
    [InlineData("", 10)]
    public void Filter_TitleAndDatesFromTo_Success(string title, int daysMargin = 0)
    {
        // Arrange
        var events = GetThreeTestEvents();
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(title, _startAt.AddDays(-daysMargin), _endAtValid.AddDays(daysMargin));

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(1);
        paginatedResult.CurrentPageEvents.First().Title.Should().Be(_currentMonthTitle);
    }

    private EventRequestDto GetValidEvent()
    {
        return new EventRequestDto("test title", "", _startAt, _endAtValid);
    }

    private EventRequestDto GetAnotherValidEvent()
    {
        return new EventRequestDto("sample title", "sample description", _startAt.AddDays(1), _endAtValid.AddDays(1));
    }

    private EventRequestDto GetEventWithInvalidDates()
    {
        return new EventRequestDto("test title", "", _startAt, _endAtInvalid);
    }

    private List<EventRequestDto> GetThreeTestEvents()
    {
        return
        [
            new(_currentMonthTitle, "", _startAt, _endAtValid),
            new(_nextMonthTitle, "", _startAt.AddMonths(1), _endAtValid.AddMonths(1)),
            new(_previousMonthTitle, "", _startAt.AddMonths(-1), _endAtValid.AddMonths(-1))
        ];
    }
}
