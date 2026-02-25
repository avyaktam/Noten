namespace Noten.Core.Models;

public enum ScheduleFilter
{
    Today,
    Upcoming,
    All
}

public enum RecurrencePattern
{
    None,
    Daily,
    Weekly
}

public sealed class ProjectData
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "New Project";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
    public string NotesRtf { get; set; } = "{\\rtf1\\ansi\\deff0 {\\fonttbl {\\f0 Segoe UI;}}\\f0\\fs22 }";
    public List<TodoListModel> Lists { get; set; } = [];
    public List<ScheduleEntryModel> ScheduleEntries { get; set; } = [];
}

public sealed class TodoListModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "List";
    public List<TodoItemModel> Items { get; set; } = [];
}

public sealed class TodoItemModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public int Priority { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public sealed class ScheduleEntryModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Today;
    public TimeSpan? StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string Notes { get; set; } = string.Empty;
    public Guid? LinkedTodoItemId { get; set; }
    public RecurrencePattern Recurrence { get; set; }
    public DateTime? RecurrenceUntil { get; set; }
}

public sealed class AppData
{
    public int SchemaVersion { get; set; } = 1;
    public string AppVersion { get; set; } = "0.1.0";
    public Guid ActiveProjectId { get; set; }
    public List<ProjectData> Projects { get; set; } = [];
}

public sealed class ProjectExportEnvelope
{
    public int SchemaVersion { get; set; } = 1;
    public string AppVersion { get; set; } = "0.1.0";
    public ProjectData Project { get; set; } = new();
}
