using System.Windows;

using Wpf.Ui.Controls;

namespace HTWind;

public partial class WidgetPermissionDecisionWindow : FluentWindow
{
    public WidgetPermissionDecisionWindow(string permissionKind, string widgetName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permissionKind);
        ArgumentException.ThrowIfNullOrWhiteSpace(widgetName);

        InitializeComponent();
        PermissionValueText.Text = permissionKind;
        WidgetValueText.Text = widgetName;
    }

    public bool IsAllowed { get; private set; }

    private void Allow_Click(object sender, RoutedEventArgs e)
    {
        IsAllowed = true;
        DialogResult = true;
        Close();
    }

    private void Deny_Click(object sender, RoutedEventArgs e)
    {
        IsAllowed = false;
        DialogResult = false;
        Close();
    }
}
