using System.Text.Json;
using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Infrastructure;

public sealed class SettingsStore
{
    private readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HardwareMonitor", "settings.json");
    public MonitorSettings Load()
    {
        try { return File.Exists(_path) ? JsonSerializer.Deserialize<MonitorSettings>(File.ReadAllText(_path)) ?? new() : new(); }
        catch { return new(); }
    }
    public void Save(MonitorSettings settings)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        File.WriteAllText(_path, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));
    }
}
