using FinancialManager.Application.Quotes;

namespace FinancialManager.UnitTests.TestDoubles;

internal static class TestQuotes
{
    public const string Currency = "USD";

    public static StockQuote Create(string symbol, decimal price, decimal? previousClose = null) => new(
        symbol,
        Name: $"{symbol} Inc.",
        Price: price,
        PreviousClose: previousClose,
        Currency: Currency,
        Exchange: "TEST",
        MarketTime: new DateTimeOffset(2025, 1, 15, 21, 0, 0, TimeSpan.Zero));
}
