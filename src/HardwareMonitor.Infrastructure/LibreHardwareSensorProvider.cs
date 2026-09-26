using HardwareMonitor.Core.Models;
using HardwareMonitor.Core.Services;
using LibreHardwareMonitor.Hardware;

namespace HardwareMonitor.Infrastructure;

public sealed class LibreHardwareSensorProvider : ISensorProvider
{
    private readonly Computer _computer;
    private readonly CoreTempSharedMemoryReader _coreTempReader = new();
    private bool _disposed;

    public LibreHardwareSensorProvider()
    {
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = true,
            IsNetworkEnabled = true,
            IsStorageEnabled = true
        };
        _computer.Open();
    }

    public Task<HardwareSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();

        var readings = new List<SensorReading>();
        var timestamp = DateTimeOffset.Now;
        try
        {
            foreach (var hardware in _computer.Hardware)
            {
                cancellationToken.ThrowIfCancellationRequested();
                Visit(hardware, readings, timestamp, cancellationToken);
            }
            _coreTempReader.TryRead(readings, timestamp);
            return Task.FromResult(new HardwareSnapshot(readings, timestamp));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Task.FromResult(new HardwareSnapshot(readings, timestamp, ex.Message));
        }
    }

    private static void Visit(IHardware hardware, ICollection<SensorReading> readings,
        DateTimeOffset timestamp, CancellationToken cancellationToken)
    {
        hardware.Update();
        foreach (var sensor in hardware.Sensors)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var kind = MapKind(sensor.SensorType);
            if (kind == SensorKind.Unknown) continue;
            readings.Add(new SensorReading(
                hardware.Name,
                sensor.Name,
                kind,
                sensor.Value,
                SensorFormatter.UnitFor(kind),
                timestamp,
                sensor.Value is null ? "传感器无读数" : null));
        }

        foreach (var subHardware in hardware.SubHardware)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Visit(subHardware, readings, timestamp, cancellationToken);
        }
    }

    private static SensorKind MapKind(SensorType type) => type switch
    {
        SensorType.Temperature => SensorKind.Temperature,
        SensorType.Load => SensorKind.Load,
        SensorType.Fan => SensorKind.Fan,
        SensorType.Clock => SensorKind.Clock,
        SensorType.Voltage => SensorKind.Voltage,
        SensorType.Power => SensorKind.Power,
        SensorType.Data => SensorKind.Data,
        SensorType.Throughput => SensorKind.Throughput,
        _ => SensorKind.Unknown
    };

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _computer.Close();
    }
}
