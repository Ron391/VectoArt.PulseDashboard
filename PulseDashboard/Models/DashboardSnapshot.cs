namespace PulseDashboard.Models;

public sealed record ChannelShare(string Name, double Percent, string Color);

/// <summary>
/// ARCH 4 — Everything the UI needs for one frame, as ONE immutable object.
/// The service builds a new snapshot every tick and swaps the reference.
/// The UI thread only ever reads a finished snapshot, so there are no locks and no half-updated screens.
/// </summary>
public sealed record DashboardSnapshot(
    IReadOnlyList<KpiMetric> Kpis,
    IReadOnlyList<double> Traffic,
    IReadOnlyList<ChannelShare> Channels,
    IReadOnlyList<ServerNode> Servers,
    IReadOnlyList<ActivityItem> Activity,
    DateTime UpdatedAt);
