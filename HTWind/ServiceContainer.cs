using HTWind.Services;
using HTWind.ViewModels;

namespace HTWind;

public sealed class ServiceContainer
{
    public ServiceContainer()
    {
        FileDialogService = new FileDialogService();
        DeveloperModeService = new DeveloperModeService();
        WebViewEnvironmentProvider = new WebViewEnvironmentProvider();
        WidgetPermissionStateService = new WidgetPermissionStateService();
        WidgetHostApiService = new WidgetHostApiService();
        WidgetWindowFactory = new WidgetWindowFactory(
            WidgetHostApiService,
            WebViewEnvironmentProvider,
            DeveloperModeService,
            WidgetPermissionStateService
        );
        WidgetStateRepository = new WidgetStateRepository();
        WidgetGeometryService = new WidgetGeometryService();
        HtmlEditorService = new HtmlEditorService(WebViewEnvironmentProvider);
        WidgetManager = new WidgetManager(
            WidgetWindowFactory,
            WidgetStateRepository,
            WidgetGeometryService,
            HtmlEditorService,
            WidgetPermissionStateService
        );
        WidgetTemplateService = new WidgetTemplateService();
        StartupRegistrationService = new StartupRegistrationService();
        ExecutionRiskConsentService = new ExecutionRiskConsentService();
        BuiltInWidgetInitializationStateService = new BuiltInWidgetInitializationStateService();
    }

    public IFileDialogService FileDialogService { get; }

    public IWidgetHostApiService WidgetHostApiService { get; }

    public IWidgetPermissionStateService WidgetPermissionStateService { get; }

    public IDeveloperModeService DeveloperModeService { get; }

    public IWebViewEnvironmentProvider WebViewEnvironmentProvider { get; }

    public IWidgetWindowFactory WidgetWindowFactory { get; }

    public IWidgetManager WidgetManager { get; }

    public IWidgetStateRepository WidgetStateRepository { get; }

    public IWidgetGeometryService WidgetGeometryService { get; }

    public IHtmlEditorService HtmlEditorService { get; }

    public IWidgetTemplateService WidgetTemplateService { get; }

    public IStartupRegistrationService StartupRegistrationService { get; }

    public IExecutionRiskConsentService ExecutionRiskConsentService { get; }

    public IBuiltInWidgetInitializationStateService BuiltInWidgetInitializationStateService { get; }

    public MainWindowViewModel CreateMainWindowViewModel()
    {
        return new MainWindowViewModel(
            FileDialogService,
            WidgetManager,
            StartupRegistrationService,
            DeveloperModeService
        );
    }

    public IApplicationBootstrapper CreateBootstrapper(MainWindow mainWindow)
    {
        ArgumentNullException.ThrowIfNull(mainWindow);

        var themeService = new ThemeService(mainWindow);
        return new ApplicationBootstrapper(
            WidgetManager,
            WidgetTemplateService,
            BuiltInWidgetInitializationStateService,
            themeService,
            mainWindow
        );
    }
}
