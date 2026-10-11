using System.Windows;

namespace PulseDashboard.Views;

public partial class BeforeWindow : Window
{
    public BeforeWindow()
    {
        InitializeComponent();
        Closed += (_, _) => (DataContext as IDisposable)?.Dispose();
    }
}
