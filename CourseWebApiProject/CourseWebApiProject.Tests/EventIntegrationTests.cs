using CourseWebApiProject.Dto;
using CourseWebApiProject.Exceptions;
using CourseWebApiProject.Repository;
using CourseWebApiProject.Services;
using FluentAssertions;

namespace CourseWebApiProject.Tests;

public class EventIntegrationTests
{
    private readonly EventService _eventService;

    public EventIntegrationTests()
    {
        _eventService = new EventService(new InMemoryEventStore());
    }

    [Fact]
    public void Add_ValidEvent_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();

        // Act
        var response = _eventService.AddEvent(validEvent);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEvent);
    }

    [Fact]
    public void GetEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        var id = _eventService.AddEvent(validEvent).Id;

        // Act
        var response = _eventService.GetEvent(id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(validEvent);
    }

    [Fact]
    public void UpdateEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        var id = _eventService.AddEvent(validEvent).Id;
        var anotherValidEvent = EventsTestsHelper.GetAnotherValidEventDto();

        // Act
        _eventService.UpdateEvent(id, anotherValidEvent);
        var response = _eventService.GetEvent(id);

        // Assert
        response.Should().NotBeNull();
        response.Should().BeEquivalentTo(anotherValidEvent);
    }

    [Fact]
    public void RemoveEvent_ExistingId_Success()
    {
        // Arrange
        var validEvent = EventsTestsHelper.GetValidEventDto();
        var id = _eventService.AddEvent(validEvent).Id;

        // Act
        _eventService.RemoveEvent(id);

        // Assert
        Assert.Throws<EventNotFoundException>(() => _eventService.GetEvent(id));
    }

    [Fact]
    public void GetAll_ThreeEventsOnSinglePage_Success()
    {
        // Arrange
        var events = EventsTestsHelper.GetThreeTestEventDtos(DateTime.Now);
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
        var events = EventsTestsHelper.GetThreeTestEventDtos(DateTime.Now);
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
        var events = EventsTestsHelper.GetThreeTestEventDtos(DateTime.Now);
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery("NEXT", null, null);

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(1);
        paginatedResult.CurrentPageEvents.First().Title.Should().Be(EventsTestsHelper.NextMonthTitle);
    }

    [Fact]
    public void Filter_DateFrom_Success()
    {
        // Arrange
        var startAtCurrentMonth = DateTime.Now;
        var events = EventsTestsHelper.GetThreeTestEventDtos(startAtCurrentMonth);
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(null, startAtCurrentMonth, null);
        var expectedTitles = new string[] { EventsTestsHelper.CurrentMonthTitle, EventsTestsHelper.NextMonthTitle };

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
        var startAtCurrentMonth = DateTime.Now;
        var durationHours = 2;
        var events = EventsTestsHelper.GetThreeTestEventDtos(startAtCurrentMonth, durationHours);
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(null, null, startAtCurrentMonth.AddHours(durationHours));
        var expectedTitles = new string[] { EventsTestsHelper.PreviousMonthTitle, EventsTestsHelper.CurrentMonthTitle };

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
        var startAtCurrentMonth = DateTime.Now;
        var durationHours = 2;
        var events = EventsTestsHelper.GetThreeTestEventDtos(startAtCurrentMonth, 2);
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(null, startAtCurrentMonth, startAtCurrentMonth.AddHours(durationHours));

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(1);
        paginatedResult.CurrentPageEvents.First().Title.Should().Be(EventsTestsHelper.CurrentMonthTitle);
    }

    [Theory]
    [InlineData("Current")]
    [InlineData("cuRRent")]
    [InlineData("event", 1)]
    [InlineData("", 10)]
    public void Filter_TitleAndDatesFromTo_Success(string title, int daysMargin = 0)
    {
        // Arrange
        var startAtCurrentMonth = DateTime.Now;
        var durationHours = 2;
        var events = EventsTestsHelper.GetThreeTestEventDtos(startAtCurrentMonth, durationHours);
        events.ForEach(e => _eventService.AddEvent(e));
        var titleQuery = new EventsQuery(title, startAtCurrentMonth.AddDays(-daysMargin), startAtCurrentMonth.AddHours(durationHours).AddDays(daysMargin));

        // Act
        var paginatedResult = _eventService.GetEventsByQuery(titleQuery);

        // Assert
        paginatedResult.Should().NotBeNull();
        paginatedResult.EventsCount.Should().Be(1);
        paginatedResult.CurrentPageEvents.First().Title.Should().Be(EventsTestsHelper.CurrentMonthTitle);
    }
}
