using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Core.Services;

public interface ISensorProvider : IDisposable
{
    Task<HardwareSnapshot> ReadSnapshotAsync(CancellationToken cancellationToken = default);
}
