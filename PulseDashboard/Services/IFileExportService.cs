namespace PulseDashboard.Services;

/// <summary>
/// ARCH 5 — The bridge between the two worlds.
/// Razor components depend on THIS interface, never on a WPF type.
/// The WPF project supplies the real implementation (a native Save dialog).
/// </summary>
public interface IFileExportService
{
    /// <returns>The saved path, or null if the user cancelled.</returns>
    Task<string?> SaveTextAsync(string suggestedFileName, string content);
}
