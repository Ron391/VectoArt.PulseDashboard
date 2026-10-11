namespace PulseDashboard.Models;

public enum ServerStatus
{
    Healthy,
    Warning,
    Critical
}

public sealed record ServerNode(
    string Name,
    string Region,
    double Cpu,
    double Memory,
    ServerStatus Status);
