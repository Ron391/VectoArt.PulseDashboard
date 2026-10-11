using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PulseDashboard.Helpers;
using PulseDashboard.Services;

namespace PulseDashboard.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        // ARCH 3 — Build the DI container BEFORE InitializeComponent().
        // The XAML reads it with {StaticResource services}, so it must already exist.
        var services = new ServiceCollection();
        services.AddWpfBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools(); // F12 opens real browser DevTools. Your best friend when the page is blank.
#endif
        services.AddSingleton<DashboardService>();
        services.AddSingleton<IFileExportService, WpfFileExportService>();
        Resources.Add("services", services.BuildServiceProvider());

        InitializeComponent();

        BlazorView.BlazorWebViewInitialized += OnBlazorWebViewInitialized;

        // Disposing the provider disposes DashboardService, which stops the timer.
        Closed += (_, _) => (Resources["services"] as IDisposable)?.Dispose();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        DarkTitleBar.Apply(this, 0x0F, 0x19, 0x23); // same navy as --bg in theme.css
    }

    private void OnBlazorWebViewInitialized(object? sender,
        Microsoft.AspNetCore.Components.WebView.BlazorWebViewInitializedEventArgs e)
    {
        // Kills the white flash: this is the colour WebView2 paints BEFORE your CSS loads.
        e.WebView.DefaultBackgroundColor = System.Drawing.Color.FromArgb(0x0F, 0x19, 0x23);

        var settings = e.WebView.CoreWebView2.Settings;
        settings.IsStatusBarEnabled = false;
        settings.IsZoomControlEnabled = false;
#if !DEBUG
        // Release builds should feel like an app, not a web page.
        settings.AreDefaultContextMenusEnabled = false;
        settings.AreBrowserAcceleratorKeysEnabled = false;
#endif
    }
}
