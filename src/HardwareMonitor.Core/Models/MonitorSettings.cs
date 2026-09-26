namespace HardwareMonitor.Core.Models;

public sealed class MonitorSettings
{
    public double RefreshIntervalSeconds { get; set; } = 1;
    public bool StartMinimized { get; set; }
    public bool MinimizeOnClose { get; set; } = true;
    public string Theme { get; set; } = "System";
    public bool UseCoreTempSharedMemory { get; set; } = true;
    public string TemperatureUnit { get; set; } = "C";
    public int CpuWarningTemperature { get; set; } = 85;
    public int CpuCriticalTemperature { get; set; } = 95;
    public int GpuWarningTemperature { get; set; } = 85;
    public int GpuCriticalTemperature { get; set; } = 95;
}
