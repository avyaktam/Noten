using Noten.Core.Models;

namespace Noten.Core.Services;

public static class HotkeyParser
{
    private static readonly Dictionary<string, uint> Keys = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Space"] = 0x20,
        ["Tab"] = 0x09,
        ["Enter"] = 0x0D,
        ["Escape"] = 0x1B,
        ["F1"] = 0x70,
        ["F2"] = 0x71,
        ["F3"] = 0x72,
        ["F4"] = 0x73,
        ["F5"] = 0x74,
        ["F6"] = 0x75,
        ["F7"] = 0x76,
        ["F8"] = 0x77,
        ["F9"] = 0x78,
        ["F10"] = 0x79,
        ["F11"] = 0x7A,
        ["F12"] = 0x7B
    };

    public static bool TryParse(string input, out HotkeyBinding binding)
    {
        binding = HotkeyBinding.Default;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var modifiers = HotkeyModifiers.None;
        uint? virtualKey = null;

        foreach (var token in input.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            if (token.Equals("Ctrl", StringComparison.OrdinalIgnoreCase) || token.Equals("Control", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Control;
                continue;
            }

            if (token.Equals("Alt", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Alt;
                continue;
            }

            if (token.Equals("Shift", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Shift;
                continue;
            }

            if (token.Equals("Win", StringComparison.OrdinalIgnoreCase) || token.Equals("Windows", StringComparison.OrdinalIgnoreCase))
            {
                modifiers |= HotkeyModifiers.Windows;
                continue;
            }

            if (token.Length == 1)
            {
                var c = char.ToUpperInvariant(token[0]);
                if (char.IsLetterOrDigit(c))
                {
                    virtualKey = c;
                    continue;
                }
            }

            if (Keys.TryGetValue(token, out var key))
            {
                virtualKey = key;
                continue;
            }

            return false;
        }

        if (modifiers == HotkeyModifiers.None || virtualKey is null)
        {
            return false;
        }

        binding = new HotkeyBinding(modifiers, virtualKey.Value);
        return true;
    }
}
