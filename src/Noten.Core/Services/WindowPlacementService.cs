using Noten.Core.Models;

namespace Noten.Core.Services;

public static class WindowPlacementService
{
    public static WindowPlacement Clamp(WindowPlacement desired, ScreenBounds screen, double minWidth, double minHeight)
    {
        var width = Math.Max(minWidth, desired.Width);
        var height = Math.Max(minHeight, desired.Height);

        var left = desired.Left;
        var top = desired.Top;

        if (left + width < screen.Left || left > screen.Right)
        {
            left = screen.Left + 30;
        }

        if (top + height < screen.Top || top > screen.Bottom)
        {
            top = screen.Top + 30;
        }

        var maxLeft = Math.Max(screen.Left, screen.Right - width);
        var maxTop = Math.Max(screen.Top, screen.Bottom - height);

        left = Math.Clamp(left, screen.Left, maxLeft);
        top = Math.Clamp(top, screen.Top, maxTop);

        return new WindowPlacement(left, top, width, height);
    }
}
