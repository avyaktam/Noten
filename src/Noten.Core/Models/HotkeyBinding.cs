namespace Noten.Core.Models;

[Flags]
public enum HotkeyModifiers
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

public sealed record HotkeyBinding(HotkeyModifiers Modifiers, uint VirtualKey)
{
    public static HotkeyBinding Default { get; } = new(HotkeyModifiers.Control, 0x20);
}
