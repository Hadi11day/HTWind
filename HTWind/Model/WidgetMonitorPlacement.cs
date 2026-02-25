namespace HTWind;

public sealed class WidgetMonitorPlacement
{
    public string? MonitorDeviceName { get; set; }

    public double Left { get; set; }

    public double Top { get; set; }

    public double Width { get; set; }

    public double Height { get; set; }

    public DateTimeOffset LastSeenAtUtc { get; set; }
}
