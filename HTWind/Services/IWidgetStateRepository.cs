namespace HTWind.Services;

public interface IWidgetStateRepository
{
    bool HasStateFile();

    WidgetStateSnapshot Load();

    void Save(WidgetStateSnapshot snapshot);

    string CopyWidgetToManagedStorage(string sourcePath);

    string CreateManagedWidgetFile(string requestedFileName, string content);

    bool IsManagedWidgetPath(string? filePath);

    void DeleteManagedWidgetFile(string? filePath);
}
