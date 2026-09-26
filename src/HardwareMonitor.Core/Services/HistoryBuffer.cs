using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Core.Services;

public sealed class HistoryBuffer(TimeSpan retention)
{
    private readonly Dictionary<string, Queue<SensorHistoryPoint>> _data = [];
    private readonly object _gate = new();

    public void Add(string key, DateTimeOffset timestamp, float? value)
    {
        if (!value.HasValue) return;
        lock (_gate)
        {
            if (!_data.TryGetValue(key, out var queue)) _data[key] = queue = new();
            queue.Enqueue(new SensorHistoryPoint(timestamp, value.Value));
            while (queue.Count > 0 && timestamp - queue.Peek().Timestamp > retention) queue.Dequeue();
        }
    }

    public IReadOnlyList<SensorHistoryPoint> Get(string key)
    {
        lock (_gate) return _data.TryGetValue(key, out var q) ? q.ToArray() : [];
    }
}
