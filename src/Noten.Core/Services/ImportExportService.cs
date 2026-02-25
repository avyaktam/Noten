using System.Text.Json;
using Noten.Core.Models;
using Noten.Core.Storage;

namespace Noten.Core.Services;

public sealed class ImportExportService
{
    public async Task ExportProjectAsync(ProjectData project, string filePath)
    {
        var clone = Clone(project);
        var envelope = new ProjectExportEnvelope
        {
            Project = clone,
            AppVersion = "0.1.0",
            SchemaVersion = 1
        };

        await JsonStorage.WriteAtomicAsync(filePath, envelope);
    }

    public async Task<ProjectExportEnvelope> ImportProjectAsync(string filePath)
    {
        await using var stream = File.OpenRead(filePath);
        var envelope = await JsonSerializer.DeserializeAsync<ProjectExportEnvelope>(stream, JsonStorage.GetJsonOptions())
            ?? throw new InvalidDataException("Import failed: file is empty or invalid.");

        if (envelope.SchemaVersion != 1)
        {
            throw new InvalidDataException($"Unsupported schema version: {envelope.SchemaVersion}.");
        }

        Normalize(envelope.Project);

        if (envelope.Project.Id == Guid.Empty)
        {
            throw new InvalidDataException("Invalid project id in import payload.");
        }

        if (string.IsNullOrWhiteSpace(envelope.Project.Name))
        {
            throw new InvalidDataException("Project name is missing in import payload.");
        }

        return envelope;
    }

    private static ProjectData Clone(ProjectData project)
    {
        var payload = JsonSerializer.Serialize(project, JsonStorage.GetJsonOptions());
        return JsonSerializer.Deserialize<ProjectData>(payload, JsonStorage.GetJsonOptions())
               ?? throw new InvalidDataException("Project serialization failed.");
    }

    private static void Normalize(ProjectData project)
    {
        project.Lists ??= [];
        project.ScheduleEntries ??= [];
        project.NotesRtf ??= string.Empty;

        foreach (var list in project.Lists)
        {
            list.Items ??= [];
            list.Name = string.IsNullOrWhiteSpace(list.Name) ? "List" : list.Name.Trim();
        }

        foreach (var item in project.Lists.SelectMany(x => x.Items))
        {
            item.Title ??= string.Empty;
            item.Notes ??= string.Empty;
        }

        foreach (var entry in project.ScheduleEntries)
        {
            entry.Title ??= string.Empty;
            entry.Notes ??= string.Empty;
        }
    }
}
