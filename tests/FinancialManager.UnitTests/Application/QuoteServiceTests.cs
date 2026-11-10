using FinancialManager.Application.Common;
using FinancialManager.Application.Quotes;
using FinancialManager.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace FinancialManager.UnitTests.Application;

public sealed class QuoteServiceTests
{
    private readonly FakeStockQuoteProvider _provider = new();
    private readonly QuoteService _service;

    public QuoteServiceTests()
    {
        _service = new QuoteService(
            _provider,
            Options.Create(new QuoteOptions { MaxConcurrentRequests = 2 }),
            NullLogger<QuoteService>.Instance);
    }

    [Fact]
    public async Task GetQuote_NormalizesSymbol()
    {
        _provider.WithQuote("AAPL", 200m);

        var result = await _service.GetQuoteAsync(new QuoteLookupRequest { Symbol = " aapl " }, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Price.Should().Be(200m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("AA PL")]
    [InlineData("<script>")]
    public async Task GetQuote_WithInvalidSymbol_ReturnsValidationErrorWithoutCallingProvider(string symbol)
    {
        var result = await _service.GetQuoteAsync(new QuoteLookupRequest { Symbol = symbol }, CancellationToken.None);

        result.Error!.Type.Should().Be(ErrorType.Validation);
        _provider.CallCount.Should().Be(0);
    }

    [Fact]
    public async Task GetQuote_WhenUnknown_ReturnsNotFound()
    {
        var result = await _service.GetQuoteAsync(new QuoteLookupRequest { Symbol = "NOPE" }, CancellationToken.None);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task GetQuote_WhenProviderFails_ReturnsExternalServiceError()
    {
        _provider.WithFailure("AAPL");

        var result = await _service.GetQuoteAsync(new QuoteLookupRequest { Symbol = "AAPL" }, CancellationToken.None);

        result.Error!.Type.Should().Be(ErrorType.ExternalService);
    }

    [Fact]
    public async Task GetQuotes_SkipsFailuresAndDeduplicates()
    {
        _provider.WithQuote("AAPL", 200m).WithQuote("MSFT", 400m).WithFailure("BAD");

        var quotes = await _service.GetQuotesAsync(["aapl", "AAPL", "MSFT", "BAD", "UNKNOWN", " "], CancellationToken.None);

        quotes.Keys.Should().BeEquivalentTo(["AAPL", "MSFT"]);
        _provider.CallCount.Should().Be(4);
    }
}
