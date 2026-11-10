using System.Net;
using System.Text;
using FinancialManager.Application.Quotes;
using FinancialManager.Infrastructure.Quotes.YahooFinance;
using Microsoft.Extensions.Options;

namespace FinancialManager.UnitTests.Infrastructure;

public sealed class YahooFinanceQuoteProviderTests
{
    private const string BaseUrl = "https://quotes.example.test/";

    private const string SuccessPayload = """
        {
          "chart": {
            "result": [
              {
                "meta": {
                  "symbol": "AAPL",
                  "currency": "USD",
                  "longName": "Apple Inc.",
                  "fullExchangeName": "NasdaqGS",
                  "regularMarketPrice": 229.87,
                  "chartPreviousClose": 226.12,
                  "regularMarketTime": 1736971200
                }
              }
            ],
            "error": null
          }
        }
        """;

    [Fact]
    public async Task GetQuote_MapsResponse()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SuccessPayload);
        var provider = CreateProvider(handler);

        var quote = await provider.GetQuoteAsync("AAPL", CancellationToken.None);

        quote.Should().NotBeNull();
        quote!.Symbol.Should().Be("AAPL");
        quote.Name.Should().Be("Apple Inc.");
        quote.Price.Should().Be(229.87m);
        quote.PreviousClose.Should().Be(226.12m);
        quote.Currency.Should().Be("USD");
        quote.Exchange.Should().Be("NasdaqGS");
        quote.MarketTime.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1736971200));
        handler.LastRequestUri!.ToString().Should().Be($"{BaseUrl}v8/finance/chart/AAPL?interval=1d&range=1d");
    }

    [Fact]
    public async Task GetQuote_EscapesSymbolInPath()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, SuccessPayload);
        var provider = CreateProvider(handler);

        await provider.GetQuoteAsync("^GSPC", CancellationToken.None);

        handler.LastRequestUri!.AbsolutePath.Should().EndWith("/%5EGSPC");
    }

    [Fact]
    public async Task GetQuote_WhenNotFound_ReturnsNull()
    {
        var provider = CreateProvider(new StubHttpMessageHandler(
            HttpStatusCode.NotFound,
            """{"chart":{"result":null,"error":{"code":"Not Found","description":"No data found"}}}"""));

        var quote = await provider.GetQuoteAsync("NOPE", CancellationToken.None);

        quote.Should().BeNull();
    }

    [Fact]
    public async Task GetQuote_WhenResultHasNoPrice_ReturnsNull()
    {
        var provider = CreateProvider(new StubHttpMessageHandler(HttpStatusCode.OK, """{"chart":{"result":[{"meta":{}}]}}"""));

        var quote = await provider.GetQuoteAsync("AAPL", CancellationToken.None);

        quote.Should().BeNull();
    }

    [Theory]
    [InlineData(HttpStatusCode.TooManyRequests, "{}")]
    [InlineData(HttpStatusCode.OK, "not json")]
    public async Task GetQuote_OnProviderError_ThrowsQuoteProviderException(HttpStatusCode statusCode, string body)
    {
        var provider = CreateProvider(new StubHttpMessageHandler(statusCode, body));

        var act = () => provider.GetQuoteAsync("AAPL", CancellationToken.None);

        await act.Should().ThrowAsync<QuoteProviderException>();
    }

    private static YahooFinanceQuoteProvider CreateProvider(HttpMessageHandler handler)
    {
        var options = new YahooFinanceOptions
        {
            BaseUrl = BaseUrl,
            ChartPath = "v8/finance/chart",
            ChartQuery = "interval=1d&range=1d",
            UserAgent = "Tests/1.0",
        };

        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(options.BaseUrl) };
        return new YahooFinanceQuoteProvider(httpClient, Options.Create(options));
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _body;

        public StubHttpMessageHandler(HttpStatusCode statusCode, string body)
        {
            _statusCode = statusCode;
            _body = body;
        }

        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastRequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_body, Encoding.UTF8, "application/json"),
            });
        }
    }
}
