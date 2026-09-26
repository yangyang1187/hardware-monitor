using System.IO.MemoryMappedFiles;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Infrastructure;

/// <summary>Reads the official Core Temp Shared Memory interface (CoreTempSharedDataEx).</summary>
public sealed class CoreTempSharedMemoryReader
{
    private const string MappingName = "CoreTempMappingObjectEx";
    private const int CoreCountOffset = 1536;
    private const int CpuCountOffset = 1540;
    private const int TemperatureOffset = 1544;
    private const int CpuNameOffset = 2584;
    private const int FahrenheitOffset = 2684;
    private const int DeltaToTjMaxOffset = 2685;
    private const int TjMaxOffset = 1024;
    private const int MaxCores = 128;

    public bool TryRead(ICollection<SensorReading> readings, DateTimeOffset timestamp)
    {
        try
        {
            using var map = MemoryMappedFile.OpenExisting(MappingName, MemoryMappedFileRights.Read);
            using var view = map.CreateViewAccessor(0, 0, MemoryMappedFileAccess.Read);
            var coreCount = Math.Clamp(view.ReadInt32(CoreCountOffset), 0, MaxCores);
            var cpuCount = view.ReadInt32(CpuCountOffset);
            if (coreCount == 0 || cpuCount == 0) return false;

            var name = ReadAnsi(view, CpuNameOffset, 100);
            if (string.IsNullOrWhiteSpace(name)) name = "CPU (Core Temp)";
            var fahrenheit = view.ReadByte(FahrenheitOffset) != 0;
            var deltaToTjMax = view.ReadByte(DeltaToTjMaxOffset) != 0;
            var added = false;

            for (var i = 0; i < coreCount; i++)
            {
                var value = view.ReadSingle(TemperatureOffset + i * sizeof(float));
                if (float.IsNaN(value) || float.IsInfinity(value)) continue;
                if (deltaToTjMax)
                {
                    var tjMax = view.ReadInt32(TjMaxOffset + i * sizeof(uint));
                    value = tjMax - value;
                }
                if (fahrenheit) value = (value - 32f) * 5f / 9f;
                if (value is < -30 or > 150) continue;
                readings.Add(new SensorReading(name, $"Core Temp #{i + 1}", SensorKind.Temperature,
                    value, "°C", timestamp, "Core Temp Shared Memory"));
                added = true;
            }
            return added;
        }
        catch (FileNotFoundException) { return false; }
        catch (UnauthorizedAccessException) { return false; }
        catch (ArgumentException) { return false; }
    }

    private static string ReadAnsi(MemoryMappedViewAccessor view, int offset, int length)
    {
        var bytes = new byte[length];
        view.ReadArray(offset, bytes, 0, bytes.Length);
        var end = Array.IndexOf(bytes, (byte)0);
        if (end < 0) end = bytes.Length;
        return System.Text.Encoding.ASCII.GetString(bytes, 0, end).Trim();
    }
}
