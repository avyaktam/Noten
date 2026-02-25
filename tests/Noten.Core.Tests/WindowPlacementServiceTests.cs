using Noten.Core.Models;
using Noten.Core.Services;

namespace Noten.Core.Tests;

public class WindowPlacementServiceTests
{
    [Fact]
    public void Clamp_OffscreenPosition_MovesIntoScreen()
    {
        var desired = new WindowPlacement(99999, 99999, 900, 600);
        var screen = new ScreenBounds(0, 0, 1920, 1080);

        var result = WindowPlacementService.Clamp(desired, screen, 760, 520);

        Assert.InRange(result.Left, 0, 1920 - result.Width);
        Assert.InRange(result.Top, 0, 1080 - result.Height);
    }

    [Fact]
    public void Clamp_RespectsMinimumSize()
    {
        var desired = new WindowPlacement(10, 10, 100, 100);
        var screen = new ScreenBounds(0, 0, 1280, 720);

        var result = WindowPlacementService.Clamp(desired, screen, 760, 520);

        Assert.Equal(760, result.Width);
        Assert.Equal(520, result.Height);
    }
}
