using FinancialManager.Application.Abstractions;

namespace FinancialManager.UnitTests.TestDoubles;

internal sealed class FakeClock : IClock
{
    public static readonly DateTime DefaultUtcNow = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);

    public DateTime UtcNow { get; set; } = DefaultUtcNow;

    public DateOnly Today => DateOnly.FromDateTime(UtcNow);
}
