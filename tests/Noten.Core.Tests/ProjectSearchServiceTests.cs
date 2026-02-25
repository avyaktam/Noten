using Noten.Core.Models;
using Noten.Core.Services;

namespace Noten.Core.Tests;

public class ProjectSearchServiceTests
{
    [Fact]
    public void Search_FindsHitsAcrossNotesListsAndSchedule()
    {
        var project = new ProjectData
        {
            NotesRtf = "{\\rtf1\\ansi This is alpha note}",
            Lists =
            [
                new TodoListModel
                {
                    Name = "Today",
                    Items = [new TodoItemModel { Title = "Buy bananas", Notes = "alpha fruit" }]
                }
            ],
            ScheduleEntries = [new ScheduleEntryModel { Title = "Alpha sync", Notes = "meet" }]
        };

        var results = ProjectSearchService.Search(project, "alpha");

        Assert.True(results.Count >= 3);
        Assert.Contains(results, r => r.Source == SearchSource.Notes);
        Assert.Contains(results, r => r.Source == SearchSource.ListItem);
        Assert.Contains(results, r => r.Source == SearchSource.Schedule);
    }

    [Fact]
    public void Search_EmptyQuery_ReturnsNoResults()
    {
        var project = new ProjectData { NotesRtf = "{\\rtf1 test}" };

        var results = ProjectSearchService.Search(project, "   ");

        Assert.Empty(results);
    }

    [Fact]
    public void Search_HonorsMaxResults()
    {
        var project = new ProjectData();
        for (var i = 0; i < 20; i++)
        {
            project.ScheduleEntries.Add(new ScheduleEntryModel { Title = $"work item {i}" });
        }

        var results = ProjectSearchService.Search(project, "work", maxResults: 5);

        Assert.Equal(5, results.Count);
    }
}
