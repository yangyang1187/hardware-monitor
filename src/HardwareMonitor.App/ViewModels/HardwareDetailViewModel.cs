using System.Collections.ObjectModel;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.App.ViewModels;

public sealed class HardwareDetailViewModel(string hardwareName, IEnumerable<SensorReading> readings)
{
    private readonly List<SensorReading> _all = readings.ToList();
    public string HardwareName { get; } = hardwareName;
    public ObservableCollection<ReadingViewModel> Readings { get; } = new();
    public string Filter { get; set; } = "全部";
    public string Search { get; set; } = "";
    public void Refresh()
    {
        Readings.Clear();
        foreach (var x in _all.Where(Matches)) Readings.Add(new ReadingViewModel(x));
    }
    private bool Matches(SensorReading x) =>
        (Filter == "全部" || (Filter == "温度" && x.Kind == SensorKind.Temperature) || (Filter == "负载" && x.Kind == SensorKind.Load) || (Filter == "频率" && x.Kind == SensorKind.Clock) || (Filter == "功耗" && x.Kind == SensorKind.Power) || (Filter == "风扇" && x.Kind == SensorKind.Fan) || (Filter == "电压" && x.Kind == SensorKind.Voltage)) &&
        (string.IsNullOrWhiteSpace(Search) || x.SensorName.Contains(Search, StringComparison.OrdinalIgnoreCase));
}
