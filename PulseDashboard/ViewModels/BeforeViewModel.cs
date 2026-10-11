using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using PulseDashboard.Models;
using PulseDashboard.Services;

namespace PulseDashboard.ViewModels;

/// <summary>
/// View-model for the "before" window: same data, stock WPF controls, zero styling.
/// It reuses DashboardService so the before/after comparison is honest: identical numbers.
/// </summary>
public partial class BeforeViewModel : ObservableObject, IDisposable
{
    private readonly DashboardService _service = new();

    [ObservableProperty] private string _revenue = "";
    [ObservableProperty] private string _users = "";
    [ObservableProperty] private string _orders = "";
    [ObservableProperty] private string _latency = "";
    [ObservableProperty] private IReadOnlyList<ServerNode> _servers = Array.Empty<ServerNode>();
    [ObservableProperty] private IReadOnlyList<ActivityItem> _activity = Array.Empty<ActivityItem>();

    public BeforeViewModel()
    {
        Refresh();
        _service.Updated += OnUpdated;
    }

    // The service ticks on a background thread, so we hop to the UI thread before touching bound properties.
    private void OnUpdated() => Application.Current.Dispatcher.BeginInvoke(new Action(Refresh));

    private void Refresh()
    {
        var s = _service.Current;
        Revenue = s.Kpis[0].Value;
        Users = s.Kpis[1].Value;
        Orders = s.Kpis[2].Value;
        Latency = s.Kpis[3].Value;
        Servers = s.Servers;
        Activity = s.Activity;
    }

    public void Dispose()
    {
        _service.Updated -= OnUpdated;
        _service.Dispose();
    }
}
