using Noten.Core.Models;

namespace Noten.Core.Services;

public static class ScheduleService
{
    public static IReadOnlyList<ScheduleEntryModel> FilterAndSort(IEnumerable<ScheduleEntryModel> entries, ScheduleFilter filter, DateTime today)
    {
        var expanded = ExpandOccurrences(entries, today.Date.AddDays(-1), today.Date.AddMonths(6));

        var query = expanded.AsEnumerable();

        query = filter switch
        {
            ScheduleFilter.Today => query.Where(e => e.Date.Date == today.Date),
            ScheduleFilter.Upcoming => query.Where(e => e.Date.Date >= today.Date),
            _ => query
        };

        return query
            .OrderBy(e => e.Date.Date)
            .ThenBy(e => e.StartTime ?? TimeSpan.MaxValue)
            .ThenBy(e => e.Title, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<ScheduleEntryModel> ExpandOccurrences(IEnumerable<ScheduleEntryModel> entries, DateTime fromDate, DateTime toDate)
    {
        var result = new List<ScheduleEntryModel>();

        foreach (var entry in entries)
        {
            if (entry.Recurrence is RecurrencePattern.None)
            {
                if (entry.Date.Date >= fromDate.Date && entry.Date.Date <= toDate.Date)
                {
                    result.Add(CloneForDate(entry, entry.Date.Date));
                }
                continue;
            }

            var until = entry.RecurrenceUntil?.Date ?? toDate.Date;
            var cursor = entry.Date.Date;

            while (cursor <= until && cursor <= toDate.Date)
            {
                if (cursor >= fromDate.Date)
                {
                    result.Add(CloneForDate(entry, cursor));
                }

                cursor = entry.Recurrence switch
                {
                    RecurrencePattern.Daily => cursor.AddDays(1),
                    RecurrencePattern.Weekly => cursor.AddDays(7),
                    _ => cursor.AddDays(1)
                };
            }
        }

        return result;
    }

    private static ScheduleEntryModel CloneForDate(ScheduleEntryModel source, DateTime date)
    {
        return new ScheduleEntryModel
        {
            Id = source.Id,
            Title = source.Title,
            Date = date,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            Notes = source.Notes,
            LinkedTodoItemId = source.LinkedTodoItemId,
            Recurrence = source.Recurrence,
            RecurrenceUntil = source.RecurrenceUntil
        };
    }
}
