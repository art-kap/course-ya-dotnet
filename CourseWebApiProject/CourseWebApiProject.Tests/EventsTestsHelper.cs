using CourseWebApiProject.Dto;
using CourseWebApiProject.Mappings;
using CourseWebApiProject.Models;

namespace CourseWebApiProject.Tests;

public static class EventsTestsHelper
{
    public const string PreviousMonthTitle = "event in the previous month";
    public const string CurrentMonthTitle = "event in the current month";
    public const string NextMonthTitle = "event in the next month";
    public const int DefaultTotalSeats = 3;

    public static EventCreate GetValidEventCreate(int totalSeats = DefaultTotalSeats)
    {
        var startAt = DateTime.Now;
        var endAtValid = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAtValid, totalSeats);
    }

    public static EventCreate GetAnotherValidEventCreate(int totalSeats = DefaultTotalSeats)
    {
        var startAt = DateTime.Now;
        var endAtValid = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAtValid, totalSeats);
    }

    public static EventUpdate GetValidEventUpdate()
    {
        var startAt = DateTime.Now.AddDays(1);
        var endAtValid = startAt.AddHours(1);

        return new EventUpdate("sample title", "sample description", startAt, endAtValid);
    }

    public static EventCreate GetEventCreateWithInvalidDates()
    {
        var startAt = DateTime.Now;
        var endAtInvalid = startAt;

        return new EventCreate("test title", "", startAt, endAtInvalid, DefaultTotalSeats);
    }

    public static EventUpdate GetEventUpdateWithInvalidDates()
    {
        var startAt = DateTime.Now;
        var endAtInvalid = startAt;

        return new EventUpdate("test title", "", startAt, endAtInvalid);
    }

    public static EventCreate GetEventCreateWithInvalidTotalSeats()
    {
        var startAt = DateTime.Now;
        var endAtValid = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAtValid, 0);
    }

    public static List<EventCreate> GetThreeTestEventCreates(DateTime startAtCurrentMonth, int durationHours = 1)
    {
        var endAtCurrentMonth = startAtCurrentMonth.AddHours(durationHours);

        return
        [
            new(PreviousMonthTitle, "", startAtCurrentMonth.AddMonths(-1), endAtCurrentMonth.AddMonths(-1), DefaultTotalSeats),
            new(CurrentMonthTitle, "", startAtCurrentMonth, endAtCurrentMonth, DefaultTotalSeats),
            new(NextMonthTitle, "", startAtCurrentMonth.AddMonths(1), endAtCurrentMonth.AddMonths(1), DefaultTotalSeats)
        ];
    }

    public static Event GetValidEvent()
    {
        return GetValidEventCreate().ToEvent();
    }

    public static Event GetAnotherValidEvent()
    {
        return GetAnotherValidEventCreate().ToEvent();
    }
}
