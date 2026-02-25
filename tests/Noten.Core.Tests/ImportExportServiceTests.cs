using Noten.Core.Models;
using Noten.Core.Services;

namespace Noten.Core.Tests;

public class ImportExportServiceTests
{
    [Fact]
    public async Task ExportThenImport_RoundTripsProject()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"noten_{Guid.NewGuid():N}.json");
        var service = new ImportExportService();
        var project = new ProjectData
        {
            Name = "Test",
            NotesRtf = "{\\rtf1 test}",
            Lists = [new TodoListModel { Name = "Today", Items = [new TodoItemModel { Title = "A" }] }],
            ScheduleEntries = [new ScheduleEntryModel { Title = "Meet", Date = new DateTime(2026, 1, 2) }]
        };

        await service.ExportProjectAsync(project, temp);
        var imported = await service.ImportProjectAsync(temp);

        Assert.Equal(1, imported.SchemaVersion);
        Assert.Equal("Test", imported.Project.Name);
        Assert.Single(imported.Project.Lists);
        Assert.Single(imported.Project.ScheduleEntries);
    }

    [Fact]
    public async Task Import_InvalidSchema_Throws()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"noten_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(temp, "{\"schemaVersion\":999,\"project\":{\"id\":\"d290f1ee-6c54-4b01-90e6-d701748f0851\"}}");
        var service = new ImportExportService();

        await Assert.ThrowsAsync<InvalidDataException>(() => service.ImportProjectAsync(temp));
    }

    [Fact]
    public async Task Import_EmptyProjectName_Throws()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"noten_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(temp, "{\"schemaVersion\":1,\"project\":{\"id\":\"d290f1ee-6c54-4b01-90e6-d701748f0851\",\"name\":\"  \"}}");
        var service = new ImportExportService();

        await Assert.ThrowsAsync<InvalidDataException>(() => service.ImportProjectAsync(temp));
    }

    [Fact]
    public async Task Import_NullCollections_AreNormalized()
    {
        var temp = Path.Combine(Path.GetTempPath(), $"noten_{Guid.NewGuid():N}.json");
        await File.WriteAllTextAsync(temp, "{\"schemaVersion\":1,\"project\":{\"id\":\"d290f1ee-6c54-4b01-90e6-d701748f0851\",\"name\":\"Imported\",\"lists\":null,\"scheduleEntries\":null,\"notesRtf\":null}}");
        var service = new ImportExportService();

        var result = await service.ImportProjectAsync(temp);

        Assert.NotNull(result.Project.Lists);
        Assert.NotNull(result.Project.ScheduleEntries);
        Assert.NotNull(result.Project.NotesRtf);
    }
}
