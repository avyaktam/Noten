using Noten.Core.Services;

namespace Noten.Core.Tests;

public class HotkeyParserTests
{
    [Theory]
    [InlineData("Ctrl+Space")]
    [InlineData("Ctrl+Shift+K")]
    [InlineData("Alt+F2")]
    public void TryParse_ValidShortcut_ReturnsTrue(string input)
    {
        var ok = HotkeyParser.TryParse(input, out var binding);

        Assert.True(ok);
        Assert.NotEqual(0U, binding.VirtualKey);
    }

    [Theory]
    [InlineData("Space")]
    [InlineData("Ctrl+")]
    [InlineData("Cmd+Space")]
    public void TryParse_InvalidShortcut_ReturnsFalse(string input)
    {
        Assert.False(HotkeyParser.TryParse(input, out _));
    }
}
