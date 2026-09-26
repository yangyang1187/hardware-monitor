using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Core.Services;

public sealed class AlertService(IEnumerable<AlertRule> rules)
{
    private readonly Dictionary<string, DateTimeOffset> _started = [];
    private readonly Dictionary<string, DateTimeOffset> _lastRaised = [];
    private readonly IReadOnlyList<AlertRule> _rules = rules.ToList();

    public IReadOnlyList<AlertEvent> Evaluate(HardwareSnapshot snapshot)
    {
        var events = new List<AlertEvent>();
        var now = snapshot.Timestamp;
        foreach (var rule in _rules)
        {
            var value = snapshot.Readings.FirstOrDefault(x => x.HardwareName.Contains(rule.Key, StringComparison.OrdinalIgnoreCase) && x.Kind == rule.Kind && x.Value.HasValue)?.Value;
            if (!value.HasValue || value < rule.Warning) { _started.Remove(rule.Key); continue; }
            if (!_started.TryGetValue(rule.Key, out var started)) _started[rule.Key] = started = now;
            if (now - started < rule.MinimumDuration) continue;
            if (_lastRaised.TryGetValue(rule.Key, out var previous) && now - previous < rule.Cooldown) continue;
            var level = value >= rule.Critical ? "严重" : "注意";
            _lastRaised[rule.Key] = now;
            events.Add(new AlertEvent(rule.Key, rule.DisplayName, value.Value, level, now));
        }
        return events;
    }
}
