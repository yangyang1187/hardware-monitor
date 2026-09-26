using Xunit;
using HardwareMonitor.Core.Models;
using HardwareMonitor.Core.Services;

namespace HardwareMonitor.Core.Tests;

public sealed class CoreTests
{
    [Fact]
    public void MissingValue_IsDisplayedAsUnavailable()
    {
        var reading = new SensorReading("CPU", "温度", SensorKind.Temperature, null, "°C", DateTimeOffset.Now);
        Assert.False(reading.IsAvailable);
        Assert.Equal("不可用", reading.DisplayValue);
    }

    [Fact]
    public void Formatter_MapsSensorUnits()
    {
        Assert.Equal("°C", SensorFormatter.UnitFor(SensorKind.Temperature));
        Assert.Equal("%", SensorFormatter.UnitFor(SensorKind.Load));
        Assert.Equal("RPM", SensorFormatter.UnitFor(SensorKind.Fan));
    }

    [Fact]
    public void Aggregator_MergesMemoryAndNetwork()
    {
        var now = DateTimeOffset.Now;
        var snapshot = new HardwareSnapshot([
            new("Total Memory", "Load", SensorKind.Load, 40, "%", now),
            new("Virtual Memory", "Load", SensorKind.Load, 20, "%", now),
            new("以太网", "Network Utilization", SensorKind.Load, 2, "%", now),
            new("以太网", "Download Speed", SensorKind.Throughput, 100, "Mbps", now)], now);
        var result = new SensorAggregator().Aggregate(snapshot);
        Assert.Contains(result, x => x.HardwareName == "内存");
        Assert.DoesNotContain(result, x => x.HardwareName == "Virtual Memory");
        Assert.All(result.Where(x => x.HardwareName == "网络"), x => Assert.True(x.Kind is SensorKind.Load or SensorKind.Throughput));
    }

    [Fact]
    public void AlertService_RespectsMinimumDurationAndCooldown()
    {
        var rule = new AlertRule("Xeon", "CPU 温度", SensorKind.Temperature, 80, 95, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(30));
        var service = new AlertService([rule]);
        var t = DateTimeOffset.Now;
        HardwareSnapshot At(DateTimeOffset time, float value) => new([new("Intel Xeon", "Core", SensorKind.Temperature, value, "°C", time)], time);
        Assert.Empty(service.Evaluate(At(t, 90)));
        Assert.Single(service.Evaluate(At(t.AddSeconds(3), 90)));
        Assert.Empty(service.Evaluate(At(t.AddSeconds(4), 96)));
    }

    [Fact]
    public async Task RefreshService_EmitsSnapshotsAndStopsOnCancellation()
    {
        using var provider = new FakeProvider();
        await using var service = new MonitorRefreshService(provider);
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(80));
        var count = 0;
        await foreach (var _ in service.StreamAsync(TimeSpan.FromMilliseconds(5), cancellation.Token)) count++;
        Assert.True(count > 0);
        Assert.True(provider.ReadCount > 0);
    }

    private sealed class FakeProvider : ISensorProvider
    {
        public int ReadCount { get; private set; }
        public Task<HardwareSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested(); ReadCount++;
            return Task.FromResult(new HardwareSnapshot([], DateTimeOffset.Now));
        }
        public void Dispose() { }
    }
}
