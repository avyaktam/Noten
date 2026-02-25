using System.Text;
using System.Text.Json;

namespace Noten.Core.Storage;

public static class JsonStorage
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task<T> ReadOrCreateAsync<T>(string path, Func<T> defaultFactory)
    {
        if (!File.Exists(path))
        {
            var value = defaultFactory();
            await WriteAtomicAsync(path, value);
            return value;
        }

        await using var stream = File.OpenRead(path);
        var parsed = await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        return parsed ?? defaultFactory();
    }

    public static Task WriteAtomicAsync<T>(string path, T value)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        var temp = $"{path}.{Guid.NewGuid():N}.tmp";
        var json = JsonSerializer.Serialize(value, JsonOptions);
        File.WriteAllText(temp, json, Encoding.UTF8);
        File.Move(temp, path, overwrite: true);
        return Task.CompletedTask;
    }

    public static JsonSerializerOptions GetJsonOptions() => JsonOptions;
}
