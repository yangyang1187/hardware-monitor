namespace HardwareMonitor.Core.Models;

public sealed record SensorHistoryPoint(DateTimeOffset Timestamp, float Value);
