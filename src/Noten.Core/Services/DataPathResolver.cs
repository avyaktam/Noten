namespace Noten.Core.Services;

public static class DataPathResolver
{
    public const string PortableFlagFileName = "noten.portable";

    public static string ResolveAppDataDirectory(string executableDirectory, Func<string> localAppDataProvider)
    {
        if (string.IsNullOrWhiteSpace(executableDirectory))
        {
            throw new ArgumentException("Executable directory is required.", nameof(executableDirectory));
        }

        var envPortable = string.Equals(Environment.GetEnvironmentVariable("NOTEN_PORTABLE"), "1", StringComparison.OrdinalIgnoreCase);
        var flagPortable = File.Exists(Path.Combine(executableDirectory, PortableFlagFileName));

        if (envPortable || flagPortable)
        {
            return Path.Combine(executableDirectory, "NotenData");
        }

        var localAppData = localAppDataProvider();
        return Path.Combine(localAppData, "Noten");
    }
}
