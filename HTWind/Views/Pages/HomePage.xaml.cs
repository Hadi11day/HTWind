using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using HTWind.Services;

namespace HTWind.Views.Pages;

public partial class HomePage : UserControl
{
    private const string DiscussionsUrl = "https://github.com/sametcn99/HTWind/discussions";

    private readonly IWidgetManager _widgetManager;

    public HomePage(IWidgetManager widgetManager)
    {
        ArgumentNullException.ThrowIfNull(widgetManager);
        _widgetManager = widgetManager;

        InitializeComponent();
    }

    private void AddWidgetOptionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement target || target.ContextMenu is not ContextMenu menu)
        {
            return;
        }

        menu.PlacementTarget = target;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private void WidgetActionsButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement target || target.ContextMenu is not ContextMenu menu)
        {
            return;
        }

        menu.DataContext = target.DataContext;
        menu.PlacementTarget = target;
        menu.Placement = PlacementMode.Bottom;
        menu.IsOpen = true;
    }

    private void CreateWithEditorMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var createWindow = new CreateWidgetWithEditorWindow
        {
            Owner = Window.GetWindow(this),
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (createWindow.ShowDialog() != true)
        {
            return;
        }

        _widgetManager.CreateWidgetWithEditor(
            createWindow.RequestedFileName,
            createWindow.IsVisibleByDefault,
            createWindow.EnableHotReload
        );
    }

    private void FindMore_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = DiscussionsUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore failures opening external links.
        }
    }
}
