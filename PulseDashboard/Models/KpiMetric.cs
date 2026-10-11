namespace PulseDashboard.Models;

/// <summary>
/// One headline number on the dashboard (revenue, users, ...).
/// A record: immutable, value-equality, perfect for "snapshot" data that the UI only reads.
/// </summary>
public sealed record KpiMetric(
    string Label,
    string Value,
    double ChangePercent,
    IReadOnlyList<double> Trend,
    string Accent,
    bool LowerIsBetter = false);
