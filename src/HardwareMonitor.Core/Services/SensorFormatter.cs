using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Core.Services;

public static class SensorFormatter
{
    public static string UnitFor(SensorKind kind) => kind switch
    {
        SensorKind.Temperature => "°C",
        SensorKind.Load => "%",
        SensorKind.Fan => "RPM",
        SensorKind.Clock => "MHz",
        SensorKind.Voltage => "V",
        SensorKind.Power => "W",
        SensorKind.Data => "GB",
        SensorKind.Throughput => "Mbps",
        _ => ""
    };
}
