namespace HardwareMonitor.Core.Models;

public sealed record HardwareSnapshot(
    IReadOnlyList<SensorReading> Readings,
    DateTimeOffset Timestamp,
    string? Error = null)
{
    public static HardwareSnapshot Empty(string? error = null) =>
        new(Array.Empty<SensorReading>(), DateTimeOffset.Now, error);
}
