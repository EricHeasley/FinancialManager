using FinancialManager.Infrastructure.Quotes;
using FinancialManager.UnitTests.TestDoubles;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace FinancialManager.UnitTests.Infrastructure;

public sealed class CachingStockQuoteProviderTests : IDisposable
{
    private readonly MemoryCache _cache = new(new MemoryCacheOptions());

    public void Dispose() => _cache.Dispose();

    [Fact]
    public async Task GetQuote_ReusesCachedQuote()
    {
        var inner = new FakeStockQuoteProvider().WithQuote("AAPL", 200m);
        var provider = CreateProvider(inner, durationSeconds: 60);

        await provider.GetQuoteAsync("AAPL", CancellationToken.None);
        var second = await provider.GetQuoteAsync("AAPL", CancellationToken.None);

        second!.Price.Should().Be(200m);
        inner.CallCount.Should().Be(1);
    }

    [Fact]
    public async Task GetQuote_DoesNotCacheMissingQuotes()
    {
        var inner = new FakeStockQuoteProvider();
        var provider = CreateProvider(inner, durationSeconds: 60);

        await provider.GetQuoteAsync("NOPE", CancellationToken.None);
        await provider.GetQuoteAsync("NOPE", CancellationToken.None);

        inner.CallCount.Should().Be(2);
    }

    [Fact]
    public async Task GetQuote_WithZeroDuration_DisablesCaching()
    {
        var inner = new FakeStockQuoteProvider().WithQuote("AAPL", 200m);
        var provider = CreateProvider(inner, durationSeconds: 0);

        await provider.GetQuoteAsync("AAPL", CancellationToken.None);
        await provider.GetQuoteAsync("AAPL", CancellationToken.None);

        inner.CallCount.Should().Be(2);
    }

    private CachingStockQuoteProvider CreateProvider(FakeStockQuoteProvider inner, int durationSeconds) =>
        new(inner, _cache, Options.Create(new QuoteCacheOptions { DurationSeconds = durationSeconds }));
}
