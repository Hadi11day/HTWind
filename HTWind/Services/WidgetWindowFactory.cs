namespace HTWind.Services;

public class WidgetWindowFactory : IWidgetWindowFactory
{
    private readonly IDeveloperModeService _developerModeService;
    private readonly IWidgetHostApiService _widgetHostApiService;
    private readonly IWebViewEnvironmentProvider _webViewEnvironmentProvider;

    public WidgetWindowFactory(
        IWidgetHostApiService widgetHostApiService,
        IWebViewEnvironmentProvider webViewEnvironmentProvider,
        IDeveloperModeService developerModeService
    )
    {
        _widgetHostApiService =
            widgetHostApiService ?? throw new ArgumentNullException(nameof(widgetHostApiService));
        _webViewEnvironmentProvider =
            webViewEnvironmentProvider
            ?? throw new ArgumentNullException(nameof(webViewEnvironmentProvider));
        _developerModeService =
            developerModeService
            ?? throw new ArgumentNullException(nameof(developerModeService));
    }

    public WidgetWindow Create(WidgetModel model)
    {
        return new WidgetWindow(
            model,
            _widgetHostApiService,
            _webViewEnvironmentProvider,
            _developerModeService
        );
    }
}
