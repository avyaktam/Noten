namespace Noten.Core.Models;

public enum SearchSource
{
    Notes,
    ListItem,
    Schedule
}

public sealed record SearchResult(SearchSource Source, string Title, string Snippet);
