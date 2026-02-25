using Noten.Core.Services;

namespace Noten.Core.Tests;

public class ProjectRepositoryTests
{
    [Fact]
    public async Task Load_WhenFileMissing_CreatesDefaultProject()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "noten_tests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(tempDir, "appdata.json");
        var repo = new ProjectRepository(path);

        var data = await repo.LoadAsync();

        Assert.NotEqual(Guid.Empty, data.ActiveProjectId);
        Assert.NotEmpty(data.Projects);
        Assert.True(File.Exists(path));
    }
}
