using FinancialManager.Application.Abstractions;

namespace FinancialManager.Infrastructure.Time;

/// <summary>
/// <see cref="IClock"/> backed by the framework <see cref="TimeProvider"/>.
/// </summary>
internal sealed class SystemClock : IClock
{
    private readonly TimeProvider _timeProvider;

    public SystemClock(TimeProvider timeProvider)
    {
        _timeProvider = timeProvider;
    }

    public DateTime UtcNow => _timeProvider.GetUtcNow().UtcDateTime;

    /// <summary>The calendar date in the server's local time zone.</summary>
    public DateOnly Today => DateOnly.FromDateTime(_timeProvider.GetLocalNow().DateTime);
}
