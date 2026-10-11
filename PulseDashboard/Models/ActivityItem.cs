namespace PulseDashboard.Models;

public enum ActivityKind
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>Id gives Blazor a stable @key so it can reuse DOM rows instead of rebuilding the list.</summary>
public sealed record ActivityItem(long Id, DateTime Time, string Message, ActivityKind Kind);
