namespace HTWind.Services;

public interface IBuiltInWidgetInitializationStateService
{
    bool HasInitialized();

    void MarkInitialized();
}
