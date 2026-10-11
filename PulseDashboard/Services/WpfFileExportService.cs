using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Win32;

namespace PulseDashboard.Services;

/// <summary>The WPF half of the bridge. Only this file knows that a native Save dialog exists.</summary>
public sealed class WpfFileExportService : IFileExportService
{
    public Task<string?> SaveTextAsync(string suggestedFileName, string content)
    {
        // Dialogs must open on the WPF UI thread, so we hop onto the Dispatcher explicitly.
        return Application.Current.Dispatcher.InvokeAsync<string?>(() =>
        {
            var dialog = new SaveFileDialog
            {
                Title = "Export dashboard data",
                FileName = suggestedFileName,
                Filter = "CSV file (*.csv)|*.csv"
            };

            if (dialog.ShowDialog(Application.Current.MainWindow) != true)
                return null;

            File.WriteAllText(dialog.FileName, content, Encoding.UTF8);
            return dialog.FileName;
        }).Task;
    }
}
