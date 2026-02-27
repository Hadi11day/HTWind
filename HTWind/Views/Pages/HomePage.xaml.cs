using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;

using HTWind.Services;

namespace HTWind.Views.Pages;

public partial class HomePage : UserControl
{
    private const string DiscussionsUrl = "https://github.com/sametcn99/HTWind/discussions";

    private readonly IWidgetManager _widgetManager;
    private ICollectionView? _widgetsCollectionView;

    public HomePage(IWidgetManager widgetManager)
    {
        ArgumentNullException.ThrowIfNull(widgetManager);
        _widgetManager = widgetManager;

        InitializeComponent();
        Loaded += HomePage_Loaded;
    }

    private void HomePage_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= HomePage_Loaded;
        EnsureWidgetsCollectionView();
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

        menu.DataContext = DataContext;
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

    private void WidgetSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyWidgetSearchFilter();
    }

    private void ApplyWidgetSearchFilter()
    {
        if (!EnsureWidgetsCollectionView())
        {
            return;
        }

        _widgetsCollectionView?.Refresh();
    }

    private bool EnsureWidgetsCollectionView()
    {
        if (_widgetsCollectionView != null)
        {
            return true;
        }

        if (WidgetsList.ItemsSource is null)
        {
            return false;
        }

        _widgetsCollectionView = CollectionViewSource.GetDefaultView(WidgetsList.ItemsSource);
        if (_widgetsCollectionView is null)
        {
            return false;
        }

        _widgetsCollectionView.Filter = FilterWidget;
        return true;
    }

    private bool FilterWidget(object item)
    {
        if (item is not WidgetModel widget)
        {
            return false;
        }

        var query = WidgetSearchTextBox.Text?.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            return true;
        }

        return (widget.DisplayName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || (widget.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false);
    }
}
