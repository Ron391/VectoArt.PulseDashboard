using System.Windows;
using PulseDashboard.Views;

namespace PulseDashboard;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // "PulseDashboard.exe --before" opens the plain stock-WPF version for the before/after demo.
        var showBefore = e.Args.Any(a => string.Equals(a, "--before", StringComparison.OrdinalIgnoreCase));
        Window window = new MainWindow();
        window.Show();
    }
}
