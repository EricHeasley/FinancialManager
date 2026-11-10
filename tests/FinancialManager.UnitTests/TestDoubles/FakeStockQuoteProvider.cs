using FinancialManager.Application.Abstractions;
using FinancialManager.Application.Quotes;

namespace FinancialManager.UnitTests.TestDoubles;

internal sealed class FakeStockQuoteProvider : IStockQuoteProvider
{
    private readonly Dictionary<string, StockQuote> _quotes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _failingSymbols = new(StringComparer.Ordinal);

    private int _callCount;

    public int CallCount => Volatile.Read(ref _callCount);

    public FakeStockQuoteProvider WithQuote(string symbol, decimal price, decimal? previousClose = null)
    {
        _quotes[symbol] = TestQuotes.Create(symbol, price, previousClose);
        return this;
    }

    public FakeStockQuoteProvider WithFailure(string symbol)
    {
        _failingSymbols.Add(symbol);
        return this;
    }

    public Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken)
    {
        Interlocked.Increment(ref _callCount);
        if (_failingSymbols.Contains(symbol))
        {
            throw new QuoteProviderException("Simulated provider failure.");
        }

        return Task.FromResult(_quotes.GetValueOrDefault(symbol));
    }
}
