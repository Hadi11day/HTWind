namespace HTWind.Services;

public interface IDeveloperModeService
{
    bool IsEnabled();

    void SetEnabled(bool enabled);

    event EventHandler? Changed;
}
