namespace FinancialManager.Application.Quotes;

/// <summary>
/// A point-in-time market price for a security.
/// </summary>
public sealed record StockQuote(
    string Symbol,
    string? Name,
    decimal Price,
    decimal? PreviousClose,
    string Currency,
    string? Exchange,
    DateTimeOffset MarketTime)
{
    public decimal? Change => PreviousClose is { } previous ? Price - previous : null;

    public decimal? ChangePercent => PreviousClose is { } previous && previous != decimal.Zero
        ? (Price - previous) / previous
        : null;
}
