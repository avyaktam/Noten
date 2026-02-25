using Noten.Core.Models;
using Noten.Core.Services;

namespace Noten.Core.Tests;

public class ScheduleServiceTests
{
    [Fact]
    public void FilterAndSort_Today_OnlyReturnsTodaySortedByStartTime()
    {
        var today = new DateTime(2026, 5, 3);
        var entries = new[]
        {
            new ScheduleEntryModel { Title = "B", Date = today, StartTime = new TimeSpan(10, 0, 0) },
            new ScheduleEntryModel { Title = "A", Date = today, StartTime = new TimeSpan(9, 0, 0) },
            new ScheduleEntryModel { Title = "Tomorrow", Date = today.AddDays(1), StartTime = new TimeSpan(8, 0, 0) }
        };

        var result = ScheduleService.FilterAndSort(entries, ScheduleFilter.Today, today);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Title);
        Assert.Equal("B", result[1].Title);
    }

    [Fact]
    public void FilterAndSort_Upcoming_ExcludesPastDates()
    {
        var today = new DateTime(2026, 5, 3);
        var entries = new[]
        {
            new ScheduleEntryModel { Title = "Past", Date = today.AddDays(-1) },
            new ScheduleEntryModel { Title = "Today", Date = today },
            new ScheduleEntryModel { Title = "Future", Date = today.AddDays(2) }
        };

        var result = ScheduleService.FilterAndSort(entries, ScheduleFilter.Upcoming, today);

        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, x => x.Title == "Past");
    }

    [Fact]
    public void ExpandOccurrences_DailyRecurrence_ExpandsIntoRange()
    {
        var entry = new ScheduleEntryModel
        {
            Title = "Daily standup",
            Date = new DateTime(2026, 5, 1),
            Recurrence = RecurrencePattern.Daily,
            RecurrenceUntil = new DateTime(2026, 5, 3)
        };

        var occurrences = ScheduleService.ExpandOccurrences([entry], new DateTime(2026, 5, 1), new DateTime(2026, 5, 7));

        Assert.Equal(3, occurrences.Count);
        Assert.Equal(new DateTime(2026, 5, 1), occurrences[0].Date);
        Assert.Equal(new DateTime(2026, 5, 3), occurrences[2].Date);
    }

    [Fact]
    public void ExpandOccurrences_WeeklyRecurrence_UsesSevenDaySteps()
    {
        var entry = new ScheduleEntryModel
        {
            Title = "Weekly sync",
            Date = new DateTime(2026, 5, 1),
            Recurrence = RecurrencePattern.Weekly,
            RecurrenceUntil = new DateTime(2026, 5, 31)
        };

        var occurrences = ScheduleService.ExpandOccurrences([entry], new DateTime(2026, 5, 1), new DateTime(2026, 5, 31));

        Assert.Equal(5, occurrences.Count);
        Assert.Equal(new DateTime(2026, 5, 29), occurrences[^1].Date);
    }
}
