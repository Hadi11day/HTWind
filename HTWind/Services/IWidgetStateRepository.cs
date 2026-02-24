namespace HTWind.Services;

public interface IWidgetStateRepository
{
    bool HasStateFile();

    IReadOnlyList<WidgetStateRecord> Load();

    void Save(IEnumerable<WidgetStateRecord> states);

    string CopyWidgetToManagedStorage(string sourcePath);

    string CreateManagedWidgetFile(string requestedFileName, string content);

    bool IsManagedWidgetPath(string? filePath);

    void DeleteManagedWidgetFile(string? filePath);
}
