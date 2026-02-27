namespace HTWind.Services;

public interface IFileDialogService
{
    bool TryPickHtmlFiles(out IReadOnlyList<string> filePaths);
    bool TryPickHtmlFile(out string filePath);
}
