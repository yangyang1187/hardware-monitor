using HardwareMonitor.Core.Models;

namespace HardwareMonitor.App.ViewModels;

public sealed class HardwareCardViewModel
{
    private readonly IReadOnlyList<SensorReading> _readings;
    private bool IsMemory => HardwareName == "内存";
    private bool IsNetwork => HardwareName == "网络" || _readings.Any(x => x.Kind == SensorKind.Throughput);

    public HardwareCardViewModel(string hardwareName, IReadOnlyList<SensorReading> readings)
    {
        HardwareName = hardwareName;
        _readings = readings;
    }

    public string HardwareName { get; }
    public string HardwareType => IsNetwork ? "网络" : IsMemory ? "内存" :
        HardwareName.Contains("cpu", StringComparison.OrdinalIgnoreCase) || HardwareName.Contains("processor", StringComparison.OrdinalIgnoreCase) || HardwareName.Contains("xeon", StringComparison.OrdinalIgnoreCase) ? "处理器" :
        HardwareName.Contains("gpu", StringComparison.OrdinalIgnoreCase) || HardwareName.Contains("graphics", StringComparison.OrdinalIgnoreCase) ? "显卡" : "硬件设备";
    public string Icon => IsNetwork ? "⌁" : IsMemory ? "▦" : HardwareType == "处理器" ? "◉" : HardwareType == "显卡" ? "▣" : "▤";
    public string Accent => HardwareType switch { "处理器" => "#55D6BE", "显卡" => "#8B9CFF", "内存" => "#F4C95D", "网络" => "#5BB7FF", _ => "#A7B0C0" };

    public string Temperature => ValueText(SensorKind.Temperature, "°C");
    public string Load => ValueText(SensorKind.Load, "%");
    public string Clock => ValueText(SensorKind.Clock, "MHz");
    public string Fan => ValueText(SensorKind.Fan, "RPM");
    public string Power => ValueText(SensorKind.Power, "W");
    public string Throughput => ValueText(SensorKind.Throughput, "Mbps");
    public double LoadValue => Math.Clamp(Value(SensorKind.Load) ?? 0, 0, 100);
    public double TemperatureValue => Value(SensorKind.Temperature) ?? 0;

    public bool ShowTemperature => !IsMemory && !IsNetwork && HasValue(SensorKind.Temperature);
    public bool ShowLoad => HasValue(SensorKind.Load);
    public bool ShowClock => !IsMemory && !IsNetwork && HasValue(SensorKind.Clock);
    public bool ShowFan => !IsMemory && !IsNetwork && HasValue(SensorKind.Fan);
    public bool ShowPower => !IsMemory && !IsNetwork && HasValue(SensorKind.Power);
    public bool ShowThroughput => IsNetwork && HasValue(SensorKind.Throughput);
    public bool ShowProgress => ShowLoad && !IsNetwork;

    private bool HasValue(SensorKind kind) => _readings.Any(x => x.Kind == kind && x.Value.HasValue);
    private float? Value(SensorKind kind) => _readings.FirstOrDefault(x => x.Kind == kind && x.Value.HasValue)?.Value;
    private string ValueText(SensorKind kind, string unit) { var value = Value(kind); return value is null ? "—" : $"{value:0.#} {unit}"; }
}
