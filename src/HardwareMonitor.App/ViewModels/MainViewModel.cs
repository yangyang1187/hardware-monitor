using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using HardwareMonitor.Core.Models;
using HardwareMonitor.Core.Services;

namespace HardwareMonitor.App.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly SensorAggregator _aggregator = new();
    private string _status = "正在初始化…";
    private string _lastUpdated = "尚未采集";
    private bool _isRefreshing;
    public ObservableCollection<HardwareCardViewModel> Cards { get; } = [];
    public string Status { get => _status; set => Set(ref _status, value); }
    public string LastUpdated { get => _lastUpdated; set => Set(ref _lastUpdated, value); }
    public bool IsRefreshing { get => _isRefreshing; set => Set(ref _isRefreshing, value); }

    public void Apply(HardwareSnapshot snapshot)
    {
        Cards.Clear();
        var readings = _aggregator.Aggregate(snapshot);
        foreach (var group in readings.GroupBy(HardwareKey).OrderBy(x => HardwareSortOrder(x.Key)))
        {
            var name = group.Key switch { "__CPU__" => group.First().HardwareName, "__MEMORY__" => "内存", "__NETWORK__" => "网络", _ => group.First().HardwareName };
            Cards.Add(new HardwareCardViewModel(name, group.ToList()));
        }
        Status = snapshot.Error is null ? $"已读取 {readings.Count} 个传感器，分为 {Cards.Count} 个硬件" : $"读取部分失败：{snapshot.Error}";
        LastUpdated = $"最后更新：{snapshot.Timestamp:yyyy-MM-dd HH:mm:ss}";
    }

    private static string HardwareKey(SensorReading reading)
    {
        var n = reading.HardwareName;
        if (n.Contains("cpu", StringComparison.OrdinalIgnoreCase) || n.Contains("processor", StringComparison.OrdinalIgnoreCase) || n.Contains("xeon", StringComparison.OrdinalIgnoreCase)) return "__CPU__";
        if (n.Contains("memory", StringComparison.OrdinalIgnoreCase) || n == "内存") return "__MEMORY__";
        if (reading.Kind == SensorKind.Throughput || n.Contains("network", StringComparison.OrdinalIgnoreCase) || n.Contains("以太", StringComparison.OrdinalIgnoreCase) || n.Contains("mihomo", StringComparison.OrdinalIgnoreCase) || n == "网络") return "__NETWORK__";
        return n;
    }
    private static int HardwareSortOrder(string key) => key switch { "__CPU__" => 0, "__MEMORY__" => 1, "__NETWORK__" => 20, _ => 10 };
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null) { if (EqualityComparer<T>.Default.Equals(field, value)) return; field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
}
