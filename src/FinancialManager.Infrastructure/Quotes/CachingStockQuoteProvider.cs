using FinancialManager.Application.Abstractions;
using FinancialManager.Application.Quotes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FinancialManager.Infrastructure.Quotes;

/// <summary>
/// Decorator that caches successful quotes to limit calls to the external provider.
/// </summary>
internal sealed class CachingStockQuoteProvider : IStockQuoteProvider
{
    private const string CacheKeyPrefix = "quote:";

    private readonly IStockQuoteProvider _inner;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheDuration;

    public CachingStockQuoteProvider(IStockQuoteProvider inner, IMemoryCache cache, IOptions<QuoteCacheOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _inner = inner;
        _cache = cache;
        _cacheDuration = TimeSpan.FromSeconds(options.Value.DurationSeconds);
    }

    public async Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeyPrefix + symbol;
        if (_cache.TryGetValue(cacheKey, out StockQuote? cached) && cached is not null)
        {
            return cached;
        }

        var quote = await _inner.GetQuoteAsync(symbol, cancellationToken);
        if (quote is not null && _cacheDuration > TimeSpan.Zero)
        {
            _cache.Set(cacheKey, quote, _cacheDuration);
        }

        return quote;
    }
}
