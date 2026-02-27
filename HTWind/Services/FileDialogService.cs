using HTWind.Localization;

using Microsoft.Win32;

namespace HTWind.Services;

public class FileDialogService : IFileDialogService
{
    public bool TryPickHtmlFiles(out IReadOnlyList<string> filePaths)
    {
        var dialog = new OpenFileDialog
        {
            Filter = LocalizationService.Get("MainWindow_FileDialogFilter"),
            Title = LocalizationService.Get("MainWindow_FileDialogTitle"),
            Multiselect = true
        };

        var result = dialog.ShowDialog() == true;
        filePaths = result
            ? dialog.FileNames.Where(path => !string.IsNullOrWhiteSpace(path)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray()
            : [];
        return result && filePaths.Count > 0;
    }

    public bool TryPickHtmlFile(out string filePath)
    {
        var result = TryPickHtmlFiles(out var filePaths);
        filePath = result ? filePaths[0] : string.Empty;
        return result;
    }
}
