using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Documents;
using Noten.App.Services;
using Noten.Core.Models;
using Noten.Core.Services;

namespace Noten.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly ProjectRepository _repository;
    private readonly ImportExportService _importExportService;
    private readonly DebounceDispatcher _debounce = new();

    private AppData _appData = new();
    private ProjectData? _activeProject;
    private AppSettings _settings = new();
    private ScheduleFilter _selectedScheduleFilter = ScheduleFilter.Upcoming;
    private string _searchQuery = string.Empty;

    public ObservableCollection<ProjectData> Projects { get; } = [];
    public ObservableCollection<ScheduleEntryModel> VisibleScheduleEntries { get; } = [];
    public ObservableCollection<SearchResult> SearchResults { get; } = [];

    public ProjectData? ActiveProject
    {
        get => _activeProject;
        set
        {
            if (_activeProject == value) return;
            _activeProject = value;
            OnPropertyChanged();
            RefreshVisibleSchedule();
            RefreshSearch();
            SaveSoon();
            DeleteProjectCommand.RaiseCanExecuteChanged();
            DuplicateProjectCommand.RaiseCanExecuteChanged();
            ExportProjectCommand.RaiseCanExecuteChanged();
        }
    }

    public ScheduleFilter SelectedScheduleFilter
    {
        get => _selectedScheduleFilter;
        set
        {
            if (_selectedScheduleFilter == value) return;
            _selectedScheduleFilter = value;
            OnPropertyChanged();
            RefreshVisibleSchedule();
            RefreshSearch();
        }
    }



    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            var next = (value ?? string.Empty).TrimStart();
            if (_searchQuery == next) return;
            _searchQuery = next;
            OnPropertyChanged();
            RefreshSearch();
        }
    }

    public AppSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
            OnPropertyChanged();
        }
    }

    public RelayCommand AddProjectCommand { get; }
    public RelayCommand DeleteProjectCommand { get; }
    public RelayCommand DuplicateProjectCommand { get; }
    public RelayCommand ExportProjectCommand { get; }
    public RelayCommand AddListCommand { get; }
    public RelayCommand AddScheduleCommand { get; }
    public RelayCommand AddTodoItemCommand { get; }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainViewModel(string dataPath)
    {
        _repository = new ProjectRepository(dataPath);
        _importExportService = new ImportExportService();

        AddProjectCommand = new RelayCommand(_ => AddProject());
        DeleteProjectCommand = new RelayCommand(_ => { }, _ => ActiveProject is not null && Projects.Count > 1);
        DuplicateProjectCommand = new RelayCommand(_ => DuplicateActiveProject(), _ => ActiveProject is not null);
        ExportProjectCommand = new RelayCommand(_ => { }, _ => ActiveProject is not null);
        AddListCommand = new RelayCommand(_ => AddList(), _ => ActiveProject is not null);
        AddScheduleCommand = new RelayCommand(_ => AddSchedule(), _ => ActiveProject is not null);
        AddTodoItemCommand = new RelayCommand(list => AddTodoItem(list as TodoListModel));
    }

    public async Task InitializeAsync()
    {
        _appData = await _repository.LoadAsync();
        Projects.Clear();

        foreach (var project in _appData.Projects)
        {
            Projects.Add(project);
        }

        ActiveProject = Projects.FirstOrDefault(p => p.Id == _appData.ActiveProjectId) ?? Projects.FirstOrDefault();
        RefreshSearch();
    }

    public bool DeleteActiveProject()
    {
        if (ActiveProject is null || Projects.Count <= 1)
        {
            return false;
        }

        var doomed = ActiveProject;
        Projects.Remove(doomed);
        _appData.Projects.Remove(doomed);
        ActiveProject = Projects.FirstOrDefault();
        SaveSoon();
        RefreshSearch();
        return true;
    }

    public bool RenameActiveProject(string projectName)
    {
        if (ActiveProject is null)
        {
            return false;
        }

        var trimmed = (projectName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return false;
        }

        ActiveProject.Name = trimmed;
        OnPropertyChanged(nameof(ActiveProject));
        SaveSoon();
        RefreshSearch();
        return true;
    }

    public ProjectData? DuplicateActiveProject()
    {
        if (ActiveProject is null)
        {
            return null;
        }

        var duplicate = new ProjectData
        {
            Name = $"{ActiveProject.Name} Copy",
            NotesRtf = ActiveProject.NotesRtf,
            Lists = ActiveProject.Lists.Select(CloneList).ToList(),
            ScheduleEntries = ActiveProject.ScheduleEntries.Select(CloneScheduleEntry).ToList()
        };

        Projects.Add(duplicate);
        _appData.Projects.Add(duplicate);
        ActiveProject = duplicate;
        SaveSoon();
        RefreshSearch();
        return duplicate;
    }

    public async Task ExportProjectAsync(string filePath)
    {
        if (ActiveProject is null)
        {
            return;
        }

        await _importExportService.ExportProjectAsync(ActiveProject, filePath);
    }

    public async Task<ImportProjectResult> ImportProjectAsync(string filePath)
    {
        var envelope = await _importExportService.ImportProjectAsync(filePath);
        var existing = Projects.FirstOrDefault(p => p.Id == envelope.Project.Id);
        if (existing is null)
        {
            Projects.Add(envelope.Project);
            _appData.Projects.Add(envelope.Project);
            ActiveProject = envelope.Project;
            SaveSoon();
            RefreshSearch();
            return new ImportProjectResult(ProjectImportConflict.None, envelope.Project);
        }

        return new ImportProjectResult(ProjectImportConflict.IdConflict, envelope.Project);
    }

    public void ResolveImportConflict(ProjectData importedProject, bool replace)
    {
        var existing = Projects.FirstOrDefault(p => p.Id == importedProject.Id);
        if (existing is null)
        {
            Projects.Add(importedProject);
            _appData.Projects.Add(importedProject);
            ActiveProject = importedProject;
            SaveSoon();
            return;
        }

        if (replace)
        {
            var idx = Projects.IndexOf(existing);
            Projects[idx] = importedProject;
            _appData.Projects[idx] = importedProject;
            ActiveProject = importedProject;
        }
        else
        {
            importedProject.Id = Guid.NewGuid();
            importedProject.Name = string.IsNullOrWhiteSpace(importedProject.Name) ? "Imported Project" : $"{importedProject.Name} (Imported)";
            Projects.Add(importedProject);
            _appData.Projects.Add(importedProject);
            ActiveProject = importedProject;
        }

        SaveSoon();
        RefreshSearch();
    }

    public void SaveSoon()
    {
        _debounce.Debounce(TimeSpan.FromMilliseconds(400), async () => await SaveAsync());
    }

    public async Task SaveAsync()
    {
        if (ActiveProject is null) return;
        _appData.ActiveProjectId = ActiveProject.Id;
        await _repository.SaveAsync(_appData);
    }

    public void UpdateNotesFromDocument(FlowDocument? doc)
    {
        if (ActiveProject is null || doc is null) return;
        var range = new TextRange(doc.ContentStart, doc.ContentEnd);
        using var ms = new MemoryStream();
        range.Save(ms, DataFormats.Rtf);
        ActiveProject.NotesRtf = System.Text.Encoding.UTF8.GetString(ms.ToArray());
        SaveSoon();
        RefreshSearch();
    }

    public void LoadDocumentTo(FlowDocument doc)
    {
        doc.Blocks.Clear();
        if (ActiveProject is null) return;

        if (string.IsNullOrWhiteSpace(ActiveProject.NotesRtf))
        {
            return;
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(ActiveProject.NotesRtf);
        using var ms = new MemoryStream(bytes);
        var range = new TextRange(doc.ContentStart, doc.ContentEnd);

        try
        {
            range.Load(ms, DataFormats.Rtf);
        }
        catch
        {
            doc.Blocks.Clear();
            doc.Blocks.Add(new Paragraph(new Run(ActiveProject.NotesRtf)));
        }
    }

    public void RefreshVisibleSchedule()
    {
        VisibleScheduleEntries.Clear();
        if (ActiveProject is null)
        {
            return;
        }

        foreach (var entry in ScheduleService.FilterAndSort(ActiveProject.ScheduleEntries, SelectedScheduleFilter, DateTime.Today))
        {
            VisibleScheduleEntries.Add(entry);
        }
    }

    private void AddProject()
    {
        var project = new ProjectData { Name = $"Project {Projects.Count + 1}" };
        Projects.Add(project);
        _appData.Projects.Add(project);
        ActiveProject = project;
        SaveSoon();
    }

    private void AddList()
    {
        ActiveProject?.Lists.Add(new TodoListModel { Name = $"List {ActiveProject.Lists.Count + 1}" });
        OnPropertyChanged(nameof(ActiveProject));
        SaveSoon();
        RefreshSearch();
    }

    private void AddTodoItem(TodoListModel? list)
    {
        if (list is null) return;
        list.Items.Add(new TodoItemModel { Title = "New item" });
        OnPropertyChanged(nameof(ActiveProject));
        SaveSoon();
        RefreshSearch();
    }

    private void AddSchedule()
    {
        ActiveProject?.ScheduleEntries.Add(new ScheduleEntryModel { Title = "New entry", Date = DateTime.Today });
        RefreshVisibleSchedule();
        OnPropertyChanged(nameof(ActiveProject));
        SaveSoon();
        RefreshSearch();
    }

    public void RefreshSearch()
    {
        SearchResults.Clear();

        foreach (var result in ProjectSearchService.Search(ActiveProject, SearchQuery, 30))
        {
            SearchResults.Add(result);
        }
    }

    private static TodoListModel CloneList(TodoListModel source)
    {
        return new TodoListModel
        {
            Name = source.Name,
            Items = source.Items.Select(CloneTodoItem).ToList()
        };
    }

    private static TodoItemModel CloneTodoItem(TodoItemModel source)
    {
        return new TodoItemModel
        {
            Title = source.Title,
            IsCompleted = source.IsCompleted,
            DueDate = source.DueDate,
            Notes = source.Notes,
            Priority = source.Priority
        };
    }

    private static ScheduleEntryModel CloneScheduleEntry(ScheduleEntryModel source)
    {
        return new ScheduleEntryModel
        {
            Title = source.Title,
            Date = source.Date,
            StartTime = source.StartTime,
            EndTime = source.EndTime,
            Notes = source.Notes,
            LinkedTodoItemId = source.LinkedTodoItemId
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

public sealed record ImportProjectResult(ProjectImportConflict Conflict, ProjectData Project);

public enum ProjectImportConflict
{
    None,
    IdConflict
}
