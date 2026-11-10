using Microsoft.AspNetCore.Components;

namespace FinancialManager.Web.Components;

/// <summary>
/// Base component that cancels in-flight work when the component is removed from the page.
/// </summary>
public abstract class CancellableComponentBase : ComponentBase, IDisposable
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private bool _disposed;

    /// <summary>Token that is cancelled when the component is disposed.</summary>
    protected CancellationToken ComponentToken => _cancellationTokenSource.Token;

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }

        _disposed = true;
    }
}
