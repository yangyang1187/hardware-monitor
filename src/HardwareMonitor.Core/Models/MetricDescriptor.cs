namespace HardwareMonitor.Core.Models;

public sealed record MetricDescriptor(string Label, string Value, string Unit, bool IsVisible, int Order = 0, string Status = "正常");
