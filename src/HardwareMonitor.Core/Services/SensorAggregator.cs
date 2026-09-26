using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Core.Services;

public sealed class SensorAggregator
{
    public IReadOnlyList<SensorReading> Aggregate(HardwareSnapshot snapshot)
    {
        var output = new List<SensorReading>();
        foreach (var group in snapshot.Readings.GroupBy(x => Key(x)))
        {
            if (group.Key == "__memory__")
            {
                var load = group.FirstOrDefault(x => x.Kind == SensorKind.Load && x.HardwareName.Contains("Total Memory", StringComparison.OrdinalIgnoreCase));
                if (load is not null) output.Add(load with { HardwareName = "内存" });
                continue;
            }
            if (group.Key == "__network__")
            {
                output.AddRange(group.Where(x => x.Kind is SensorKind.Load or SensorKind.Throughput).Select(x => x with { HardwareName = "网络" }));
                continue;
            }
            output.AddRange(group);
        }
        return output;
    }

    private static string Key(SensorReading x)
    {
        var n = x.HardwareName;
        if (n.Contains("memory", StringComparison.OrdinalIgnoreCase)) return "__memory__";
        if (x.Kind == SensorKind.Throughput || n.Contains("network", StringComparison.OrdinalIgnoreCase) || n.Contains("以太", StringComparison.OrdinalIgnoreCase) || n.Contains("mihomo", StringComparison.OrdinalIgnoreCase)) return "__network__";
        if (n.Contains("cpu", StringComparison.OrdinalIgnoreCase) || n.Contains("processor", StringComparison.OrdinalIgnoreCase) || n.Contains("xeon", StringComparison.OrdinalIgnoreCase)) return "__cpu__";
        return n;
    }
}
