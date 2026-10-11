using System.Globalization;
using System.Text;
using PulseDashboard.Models;

namespace PulseDashboard.Services;

/// <summary>
/// Fake-but-real live data source. Every 1.5 seconds it nudges the numbers (a "random walk"),
/// builds a fresh immutable <see cref="DashboardSnapshot"/> and raises <see cref="Updated"/>.
///
/// ARCH 3 — One service, registered as a singleton, shared by every Razor component.
/// Swap the random walk for a real API call later and no component has to change.
/// </summary>
public sealed class DashboardService : IDisposable
{
    private const int TrendLength = 24;
    private const int TrafficLength = 48;

    // Fixed seed = the same "random" data every run, so your recording matches your script.
    private readonly Random _rng = new(42);
    private readonly CancellationTokenSource _cts = new();

    // Culture is pinned on purpose: on a Danish or German PC "1,5" would break SVG coordinates.
    private static readonly CultureInfo Us = CultureInfo.GetCultureInfo("en-US");

    private double _revenue = 48_200, _users = 1_240, _orders = 312, _latency = 142, _traffic = 900;
    private readonly Queue<double> _revenueTrend = new(), _usersTrend = new(), _ordersTrend = new(),
                                   _latencyTrend = new(), _trafficSeries = new();

    private static readonly (string Name, string Region)[] ServerDefs =
    {
        ("api-eu-01", "Frankfurt"), ("api-us-01", "Virginia"), ("db-primary", "Dublin"),
        ("cache-01", "Frankfurt"), ("worker-03", "Singapore")
    };
    private readonly double[] _cpu = { 34, 58, 71, 22, 47 };
    private readonly double[] _mem = { 52, 61, 66, 38, 55 };
    private readonly bool[] _wasCritical = new bool[5];
    private readonly double[] _channelWeights = { 42, 28, 18, 12 };

    private static readonly (string Text, ActivityKind Kind)[] Events =
    {
        ("New order #{0} placed", ActivityKind.Success),
        ("Payment confirmed for order #{0}", ActivityKind.Success),
        ("User signed in from a new device", ActivityKind.Info),
        ("Deployment v2.{1} finished", ActivityKind.Info),
        ("Slow query detected on db-primary", ActivityKind.Warning),
        ("Refund requested for order #{0}", ActivityKind.Warning)
    };
    private readonly List<ActivityItem> _activity = new();
    private long _nextId = 1;

    public DashboardService()
    {
        // Pre-fill the history so the charts are full the moment the window opens.
        for (var i = 0; i < TrafficLength; i++) Advance();
        Current = BuildSnapshot();
        _ = RunAsync(_cts.Token);
    }

    /// <summary>The latest finished frame. Reference assignment is atomic, so readers never see a torn state.</summary>
    public DashboardSnapshot Current { get; private set; }

    /// <summary>Raised on a thread-pool thread. Subscribers must marshal to their own UI thread.</summary>
    public event Action? Updated;

    private async Task RunAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1500));
        try
        {
            // ConfigureAwait(false): keep ticking off the UI thread. A busy chart must never block a button click.
            while (await timer.WaitForNextTickAsync(ct).ConfigureAwait(false))
            {
                Advance();
                Current = BuildSnapshot();
                Updated?.Invoke();
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
    }

    private double Walk(double value, double step, double min, double max) =>
        Math.Clamp(value + (_rng.NextDouble() - 0.5) * 2 * step, min, max);

    private static void Push(Queue<double> queue, double value, int max)
    {
        queue.Enqueue(value);
        while (queue.Count > max) queue.Dequeue();
    }

    private void Advance()
    {
        _revenue = Walk(_revenue, 900, 30_000, 90_000);
        _users = Walk(_users, 40, 600, 3_000);
        _orders = Walk(_orders, 12, 100, 700);
        _latency = Walk(_latency, 9, 60, 320);
        _traffic = Walk(_traffic, 120, 400, 1_800);

        Push(_revenueTrend, _revenue, TrendLength);
        Push(_usersTrend, _users, TrendLength);
        Push(_ordersTrend, _orders, TrendLength);
        Push(_latencyTrend, _latency, TrendLength);
        Push(_trafficSeries, _traffic, TrafficLength);

        for (var i = 0; i < _cpu.Length; i++)
        {
            _cpu[i] = Walk(_cpu[i], 7, 8, 98);
            _mem[i] = Walk(_mem[i], 3, 20, 92);

            var critical = _cpu[i] > 85;
            if (critical && !_wasCritical[i])
                AddActivity($"{ServerDefs[i].Name} CPU at {_cpu[i]:0}%", ActivityKind.Error);
            _wasCritical[i] = critical;
        }

        for (var i = 0; i < _channelWeights.Length; i++)
            _channelWeights[i] = Walk(_channelWeights[i], 1.2, 5, 60);

        if (_rng.NextDouble() < 0.4)
        {
            var (text, kind) = Events[_rng.Next(Events.Length)];
            AddActivity(string.Format(Us, text, _rng.Next(10_000, 99_999), _rng.Next(1, 40)), kind);
        }
    }

    private void AddActivity(string message, ActivityKind kind)
    {
        _activity.Insert(0, new ActivityItem(_nextId++, DateTime.Now, message, kind));
        if (_activity.Count > 8) _activity.RemoveAt(_activity.Count - 1);
    }

    private DashboardSnapshot BuildSnapshot()
    {
        var kpis = new[]
        {
            Kpi("Revenue", string.Format(Us, "${0:N0}", _revenue), _revenueTrend, "#4A8EF5"),
            Kpi("Active users", string.Format(Us, "{0:N0}", _users), _usersTrend, "#2ECC8F"),
            Kpi("Orders", string.Format(Us, "{0:N0}", _orders), _ordersTrend, "#9B7BFF"),
            Kpi("Avg latency", string.Format(Us, "{0:0} ms", _latency), _latencyTrend, "#F5A623", lowerIsBetter: true)
        };

        var servers = new ServerNode[ServerDefs.Length];
        for (var i = 0; i < servers.Length; i++)
        {
            var status = _cpu[i] > 85 ? ServerStatus.Critical
                       : _cpu[i] > 70 ? ServerStatus.Warning
                       : ServerStatus.Healthy;
            servers[i] = new ServerNode(ServerDefs[i].Name, ServerDefs[i].Region,
                                        Math.Round(_cpu[i]), Math.Round(_mem[i]), status);
        }

        var total = _channelWeights.Sum();
        string[] names = { "Organic", "Direct", "Referral", "Social" };
        string[] colors = { "#4A8EF5", "#2ECC8F", "#9B7BFF", "#F5A623" };
        var channels = new ChannelShare[names.Length];
        for (var i = 0; i < names.Length; i++)
            channels[i] = new ChannelShare(names[i], Math.Round(_channelWeights[i] / total * 100, 1), colors[i]);

        return new DashboardSnapshot(kpis, _trafficSeries.ToArray(), channels, servers,
                                     _activity.ToArray(), DateTime.Now);
    }

    private static KpiMetric Kpi(string label, string value, Queue<double> trend, string accent,
                                 bool lowerIsBetter = false)
    {
        var points = trend.ToArray();
        var change = (points[^1] - points[0]) / points[0] * 100;
        return new KpiMetric(label, value, Math.Round(change, 1), points, accent, lowerIsBetter);
    }

    /// <summary>Used by the native "Export CSV" button: Razor asks, WPF saves the file.</summary>
    public string ToCsv()
    {
        var sb = new StringBuilder("Server,Region,CPU %,Memory %,Status\r\n");
        foreach (var n in Current.Servers)
        {
            sb.AppendLine(string.Join(',', n.Name, n.Region,
                n.Cpu.ToString("0", CultureInfo.InvariantCulture),
                n.Memory.ToString("0", CultureInfo.InvariantCulture),
                n.Status.ToString()));
        }
        return sb.ToString();
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}
