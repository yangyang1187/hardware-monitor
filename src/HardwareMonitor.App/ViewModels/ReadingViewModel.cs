using HardwareMonitor.Core.Models;

namespace HardwareMonitor.App.ViewModels;

public sealed class ReadingViewModel(SensorReading reading)
{
    public string HardwareName => reading.HardwareName;
    public string SensorName => reading.SensorName;
    public string KindName => reading.Kind switch
    {
        SensorKind.Temperature => "温度",
        SensorKind.Load => "使用率",
        SensorKind.Fan => "风扇",
        SensorKind.Clock => "频率",
        SensorKind.Voltage => "电压",
        SensorKind.Power => "功耗",
        SensorKind.Data => "容量",
        SensorKind.Throughput => "吞吐",
        _ => "其他"
    };
    public string DisplayValue => reading.DisplayValue;
    public string Status => reading.Status ?? (reading.IsAvailable ? "正常" : "不可用");
    public bool IsAvailable => reading.IsAvailable;
}
