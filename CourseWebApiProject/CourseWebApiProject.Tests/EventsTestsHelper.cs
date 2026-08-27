using CourseWebApiProject.Dto;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Tests;

public static class EventsTestsHelper
{
    public const string PreviousMonthTitle = "event in the previous month";
    public const string CurrentMonthTitle = "event in the current month";
    public const string NextMonthTitle = "event in the next month";

    public static EventRequestDto GetValidEventDto()
    {
        var startAt = DateTime.Now;
        var endAtValid = startAt.AddHours(1);

        return new EventRequestDto("test title", "", startAt, endAtValid);
    }

    public static EventRequestDto GetAnotherValidEventDto()
    {
        var startAt = DateTime.Now.AddDays(1);
        var endAtValid = startAt.AddHours(1);

        return new EventRequestDto("sample title", "sample description", startAt, endAtValid);
    }

    public static EventRequestDto GetEventDtoWithInvalidDates()
    {
        var startAt = DateTime.Now;
        var endAtInvalid = startAt;

        return new EventRequestDto("test title", "", startAt, endAtInvalid);
    }

    public static List<EventRequestDto> GetThreeTestEventDtos(DateTime startAtCurrentMonth, int durationHours = 1)
    {
        var endAtCurrentMonth = startAtCurrentMonth.AddHours(durationHours);

        return
        [
            new(PreviousMonthTitle, "", startAtCurrentMonth.AddMonths(-1), endAtCurrentMonth.AddMonths(-1)),
            new(CurrentMonthTitle, "", startAtCurrentMonth, endAtCurrentMonth),
            new(NextMonthTitle, "", startAtCurrentMonth.AddMonths(1), endAtCurrentMonth.AddMonths(1))
        ];
    }

    public static Event GetValidEvent()
    {
        return GetValidEventDto().ToEvent();
    }

    public static Event GetAnotherValidEvent()
    {
        return GetAnotherValidEventDto().ToEvent();
    }
}
