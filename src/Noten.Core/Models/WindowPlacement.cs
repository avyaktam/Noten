namespace Noten.Core.Models;

public sealed record WindowPlacement(double Left, double Top, double Width, double Height);

public sealed record ScreenBounds(double Left, double Top, double Width, double Height)
{
    public double Right => Left + Width;
    public double Bottom => Top + Height;
}
