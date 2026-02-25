using Noten.Core.Models;
using Noten.Core.Storage;

namespace Noten.Core.Services;

public sealed class SettingsRepository
{
    private readonly string _path;

    public SettingsRepository(string path)
    {
        _path = path;
    }

    public Task<AppSettings> LoadAsync() => JsonStorage.ReadOrCreateAsync(_path, () => new AppSettings());

    public Task SaveAsync(AppSettings settings) => JsonStorage.WriteAtomicAsync(_path, settings);
}
