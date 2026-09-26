namespace HardwareMonitor.Core.Models;

public sealed record AlertRule(string Key, string DisplayName, SensorKind Kind, float Warning, float Critical, TimeSpan MinimumDuration, TimeSpan Cooldown);
public sealed record AlertEvent(string Key, string DisplayName, float Value, string Level, DateTimeOffset Timestamp);
