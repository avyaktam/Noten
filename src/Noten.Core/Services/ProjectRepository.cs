using Noten.Core.Models;
using Noten.Core.Storage;

namespace Noten.Core.Services;

public sealed class ProjectRepository
{
    private readonly string _dataPath;

    public ProjectRepository(string dataPath)
    {
        _dataPath = dataPath;
    }

    public Task<AppData> LoadAsync() => JsonStorage.ReadOrCreateAsync(_dataPath, CreateDefault);

    public Task SaveAsync(AppData appData)
    {
        foreach (var project in appData.Projects)
        {
            project.UpdatedAtUtc = DateTime.UtcNow;
        }

        return JsonStorage.WriteAtomicAsync(_dataPath, appData);
    }

    private static AppData CreateDefault()
    {
        var project = new ProjectData
        {
            Name = "Personal"
        };

        return new AppData
        {
            ActiveProjectId = project.Id,
            Projects = [project]
        };
    }
}
