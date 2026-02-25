using Noten.Core.Services;

namespace Noten.Core.Tests;

public class DataPathResolverTests
{
    [Fact]
    public void ResolveAppDataDirectory_Default_UsesLocalAppData()
    {
        var exeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(exeDir);
        Environment.SetEnvironmentVariable("NOTEN_PORTABLE", null);

        var resolved = DataPathResolver.ResolveAppDataDirectory(exeDir, () => "/tmp/localapp");

        Assert.Equal(Path.Combine("/tmp/localapp", "Noten"), resolved);
    }

    [Fact]
    public void ResolveAppDataDirectory_PortableFlag_UsesExecutableDirectory()
    {
        var exeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(exeDir);
        File.WriteAllText(Path.Combine(exeDir, DataPathResolver.PortableFlagFileName), "1");
        Environment.SetEnvironmentVariable("NOTEN_PORTABLE", null);

        var resolved = DataPathResolver.ResolveAppDataDirectory(exeDir, () => "/tmp/localapp");

        Assert.Equal(Path.Combine(exeDir, "NotenData"), resolved);
    }

    [Fact]
    public void ResolveAppDataDirectory_PortableEnv_UsesExecutableDirectory()
    {
        var exeDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(exeDir);
        Environment.SetEnvironmentVariable("NOTEN_PORTABLE", "1");

        var resolved = DataPathResolver.ResolveAppDataDirectory(exeDir, () => "/tmp/localapp");

        Assert.Equal(Path.Combine(exeDir, "NotenData"), resolved);

        Environment.SetEnvironmentVariable("NOTEN_PORTABLE", null);
    }
}
