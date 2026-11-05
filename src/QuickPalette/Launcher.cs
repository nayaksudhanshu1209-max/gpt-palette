using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Runtime.InteropServices;

namespace QuickPalette;
public static class Launcher
{
    public static void Run(PaletteItem item)
    {
        try
        {
            switch (item.Action)
            {
                case "url":
                    Process.Start(new ProcessStartInfo(item.Target!) { UseShellExecute = true });
                    break;
                case "app":
                case "command":
                    Process.Start(new ProcessStartInfo(item.Target!) { UseShellExecute = true });
                    break;
                case "settings":
                    Process.Start(new ProcessStartInfo(item.Target!) { UseShellExecute = true });
                    break;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open {item.Title}.\\n{ex.Message}", "QuickPalette",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
public static class Hotkey
{
    [DllImport("user32.dll")] private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    [DllImport("user32.dll")] private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    public const int Id = 0x51A;
    public static bool Register(IntPtr hwnd, string text)
    {
        UnregisterHotKey(hwnd, Id);
        var parts = text.Split('+', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        uint mods = 0, key = 0;
        foreach (var p in parts)
        {
            switch (p.ToLowerInvariant())
            {
                case "alt": mods |= 1; break;
                case "ctrl": case "control": mods |= 2; break;
                case "shift": mods |= 4; break;
                case "win": case "windows": mods |= 8; break;
                default:
                    if (Enum.TryParse<System.Windows.Input.Key>(p, true, out var k))
                        key = (uint)System.Windows.Input.KeyInterop.VirtualKeyFromKey(k);
                    else if (p.Length == 1) key = char.ToUpperInvariant(p[0]);
                    break;
            }
        }
        return key != 0 && RegisterHotKey(hwnd, Id, mods, key);
    }
    public static void Unregister(IntPtr hwnd) => UnregisterHotKey(hwnd, Id);
}
