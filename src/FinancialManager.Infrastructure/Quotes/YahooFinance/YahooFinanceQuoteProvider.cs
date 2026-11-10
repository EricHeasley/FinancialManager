using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FinancialManager.Application.Abstractions;
using FinancialManager.Application.Quotes;
using Microsoft.Extensions.Options;
using Polly;

namespace FinancialManager.Infrastructure.Quotes.YahooFinance;

/// <summary>
/// Retrieves quotes from the public Yahoo Finance chart API. No API key is required.
/// </summary>
internal sealed class YahooFinanceQuoteProvider : IStockQuoteProvider
{
    private readonly HttpClient _httpClient;
    private readonly YahooFinanceOptions _options;

    public YahooFinanceQuoteProvider(HttpClient httpClient, IOptions<YahooFinanceOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var requestUri = new Uri(
            $"{_options.ChartPath.TrimEnd('/')}/{Uri.EscapeDataString(symbol)}?{_options.ChartQuery}",
            UriKind.Relative);

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUri, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new QuoteProviderException($"Request for '{symbol}' failed.", ex);
        }
        catch (ExecutionRejectedException ex)
        {
            // Raised by the resilience pipeline (timeout or open circuit breaker).
            throw new QuoteProviderException($"Request for '{symbol}' was rejected by the resilience policy.", ex);
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new QuoteProviderException($"Request for '{symbol}' timed out.", ex);
        }

        using (response)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                throw new QuoteProviderException(
                    $"Quote provider returned {(int)response.StatusCode} ({response.ReasonPhrase}) for '{symbol}'.");
            }

            YahooChartResponse? payload;
            try
            {
                payload = await response.Content.ReadFromJsonAsync<YahooChartResponse>(cancellationToken);
            }
            catch (JsonException ex)
            {
                throw new QuoteProviderException($"Quote provider returned an unreadable response for '{symbol}'.", ex);
            }

            return MapQuote(symbol, payload);
        }
    }

    internal static StockQuote? MapQuote(string requestedSymbol, YahooChartResponse? payload)
    {
        var results = payload?.Chart?.Result;
        var meta = results is { Count: > 0 } ? results[0].Meta : null;
        if (meta?.RegularMarketPrice is not { } price)
        {
            return null;
        }

        var marketTime = meta.RegularMarketTime is { } unixSeconds
            ? DateTimeOffset.FromUnixTimeSeconds(unixSeconds)
            : DateTimeOffset.UtcNow;

        return new StockQuote(
            Symbol: string.IsNullOrWhiteSpace(meta.Symbol) ? requestedSymbol : meta.Symbol.ToUpperInvariant(),
            Name: meta.LongName ?? meta.ShortName,
            Price: price,
            PreviousClose: meta.PreviousClose ?? meta.ChartPreviousClose,
            Currency: meta.Currency ?? string.Empty,
            Exchange: meta.FullExchangeName ?? meta.ExchangeName,
            MarketTime: marketTime);
    }
}
