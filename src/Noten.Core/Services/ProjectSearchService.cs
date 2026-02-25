using System.Text.RegularExpressions;
using Noten.Core.Models;

namespace Noten.Core.Services;

public static class ProjectSearchService
{
    private static readonly Regex RtfControlRegex = new(@"\\[a-z]+\d* ?", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private static readonly Regex BracesRegex = new(@"[{}]", RegexOptions.Compiled);
    private static readonly Regex MultiWhitespaceRegex = new(@"\s+", RegexOptions.Compiled);

    public static IReadOnlyList<SearchResult> Search(ProjectData? project, string query, int maxResults = 50)
    {
        if (project is null)
        {
            return [];
        }

        query = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return [];
        }

        var results = new List<SearchResult>();
        var comparison = StringComparison.OrdinalIgnoreCase;

        var notes = ToPlainText(project.NotesRtf);
        if (Contains(notes, query, comparison))
        {
            results.Add(new SearchResult(SearchSource.Notes, "Notes", CreateSnippet(notes, query)));
        }

        foreach (var list in project.Lists)
        {
            foreach (var item in list.Items)
            {
                var haystack = $"{item.Title} {item.Notes}";
                if (!Contains(haystack, query, comparison))
                {
                    continue;
                }

                results.Add(new SearchResult(
                    SearchSource.ListItem,
                    $"List: {list.Name}",
                    CreateSnippet(haystack, query)));

                if (results.Count >= maxResults)
                {
                    return results;
                }
            }
        }

        foreach (var entry in project.ScheduleEntries)
        {
            var haystack = $"{entry.Title} {entry.Notes}";
            if (!Contains(haystack, query, comparison))
            {
                continue;
            }

            results.Add(new SearchResult(
                SearchSource.Schedule,
                $"Schedule: {entry.Date:d}",
                CreateSnippet(haystack, query)));

            if (results.Count >= maxResults)
            {
                return results;
            }
        }

        return results;
    }

    private static bool Contains(string? text, string query, StringComparison comparison)
        => !string.IsNullOrWhiteSpace(text) && text.Contains(query, comparison);

    private static string ToPlainText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var text = RtfControlRegex.Replace(value, " ");
        text = BracesRegex.Replace(text, " ");
        text = MultiWhitespaceRegex.Replace(text, " ");
        return text.Trim();
    }

    private static string CreateSnippet(string text, string query)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var index = text.IndexOf(query, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
        {
            return text.Length <= 100 ? text : text[..100];
        }

        var start = Math.Max(0, index - 28);
        var end = Math.Min(text.Length, index + query.Length + 28);
        var snippet = text[start..end].Trim();

        if (start > 0)
        {
            snippet = $"…{snippet}";
        }

        if (end < text.Length)
        {
            snippet = $"{snippet}…";
        }

        return snippet;
    }
}
