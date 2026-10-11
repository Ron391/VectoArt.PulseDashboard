using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PulseDashboard.Helpers;

/// <summary>
/// A dark app inside a bright Windows title bar looks broken. Two DWM attributes fix it.
/// Both calls fail silently on older Windows builds, which is fine: you just keep the default bar.
/// </summary>
public static class DarkTitleBar
{
    private const int DwmUseImmersiveDarkMode = 20; // Windows 10 2004+
    private const int DwmCaptionColor = 35;         // Windows 11+

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    /// <summary>Call from OnSourceInitialized: the window handle does not exist before that.</summary>
    public static void Apply(Window window, byte r, byte g, byte b)
    {
        var hwnd = new WindowInteropHelper(window).Handle;

        var dark = 1;
        _ = DwmSetWindowAttribute(hwnd, DwmUseImmersiveDarkMode, ref dark, sizeof(int));

        // COLORREF is 0x00BBGGRR, not RRGGBB.
        var colorRef = (b << 16) | (g << 8) | r;
        _ = DwmSetWindowAttribute(hwnd, DwmCaptionColor, ref colorRef, sizeof(int));
    }
}
