using CourseWebApi.Application.Dto;
using CourseWebApi.Application.Mappings;
using CourseWebApi.Domain.Entities;

namespace CourseWebApiProject.Tests;

public static class EventsTestsHelper
{
    public const string PreviousMonthTitle = "event in the previous month";
    public const string CurrentMonthTitle = "event in the current month";
    public const string NextMonthTitle = "event in the next month";
    public const int DefaultTotalSeats = 3;

    public static EventCreate GetValidEventCreate(int totalSeats = DefaultTotalSeats)
    {
        var startAt = GetStartTime();
        var endAtValid = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAtValid, totalSeats);
    }

    public static EventCreate GetAnotherValidEventCreate(int totalSeats = DefaultTotalSeats)
    {
        var startAt = GetStartTime();
        var endAtValid = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAtValid, totalSeats);
    }

    public static EventUpdate GetValidEventUpdate()
    {
        var startAt = GetStartTime().AddDays(1);
        var endAtValid = startAt.AddHours(1);

        return new EventUpdate("sample title", "sample description", startAt, endAtValid);
    }

    public static EventCreate GetEventCreateWithInvalidDates()
    {
        var startAt = GetStartTime();
        var endAtInvalid = startAt;

        return new EventCreate("test title", "", startAt, endAtInvalid, DefaultTotalSeats);
    }

    public static EventUpdate GetEventUpdateWithInvalidDates()
    {
        var startAt = GetStartTime();
        var endAtInvalid = startAt;

        return new EventUpdate("test title", "", startAt, endAtInvalid);
    }

    public static EventCreate GetEventCreateWithInvalidTotalSeats()
    {
        var startAt = GetStartTime();
        var endAtValid = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAtValid, 0);
    }

    public static EventCreate GetPastEventCreate()
    {
        var startAt = DateTime.Now.AddMinutes(-1).ToUniversalTime();
        var endAt = startAt.AddHours(1);

        return new EventCreate("test title", "", startAt, endAt, DefaultTotalSeats);
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

    public static UserCreate GetTestUserCreate(string login = "user")
    {
        return new UserCreate(login, "", UserRole.User);
    }

    public static UserCreate GetTestAdminCreate()
    {
        return new UserCreate("admin", "", UserRole.Admin);
    }

    private static DateTime GetStartTime() => DateTime.Now.ToUniversalTime().AddHours(1);
}
