using HardwareMonitor.Core.Models;

namespace HardwareMonitor.Core.Services;

public sealed class MonitorRefreshService(ISensorProvider provider) : IAsyncDisposable
{
    private readonly ISensorProvider _provider = provider;
    private readonly SemaphoreSlim _readGate = new(1, 1);

    public async Task<HardwareSnapshot> ReadOnceAsync(CancellationToken cancellationToken = default)
    {
        await _readGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { return await _provider.ReadSnapshotAsync(cancellationToken).ConfigureAwait(false); }
        finally { _readGate.Release(); }
    }

    public async IAsyncEnumerable<HardwareSnapshot> StreamAsync(
        TimeSpan interval,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (interval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(interval));

        while (!cancellationToken.IsCancellationRequested)
        {
            HardwareSnapshot snapshot;
            try
            {
                snapshot = await ReadOnceAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }
            catch (Exception ex)
            {
                snapshot = HardwareSnapshot.Empty(ex.Message);
            }

            yield return snapshot;
            try { await Task.Delay(interval, cancellationToken).ConfigureAwait(false); }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { yield break; }
        }
    }

    public ValueTask DisposeAsync()
    {
        _provider.Dispose();
        return ValueTask.CompletedTask;
    }
}
