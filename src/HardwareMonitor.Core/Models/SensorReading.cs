namespace HardwareMonitor.Core.Models;

public sealed record SensorReading(
    string HardwareName,
    string SensorName,
    SensorKind Kind,
    float? Value,
    string Unit,
    DateTimeOffset Timestamp,
    string? Status = null)
{
    public bool IsAvailable => Value.HasValue;
    public string DisplayValue => Value is null ? "不可用" : $"{Value:0.##} {Unit}";
}
