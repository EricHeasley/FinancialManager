using System.Text.Json.Serialization;

namespace FinancialManager.Infrastructure.Quotes.YahooFinance;

// Wire contracts for the subset of the Yahoo Finance chart response that the application uses.

internal sealed record YahooChartResponse(
    [property: JsonPropertyName("chart")] YahooChart? Chart);

internal sealed record YahooChart(
    [property: JsonPropertyName("result")] IReadOnlyList<YahooChartResult>? Result,
    [property: JsonPropertyName("error")] YahooChartError? Error);

internal sealed record YahooChartResult(
    [property: JsonPropertyName("meta")] YahooChartMeta? Meta);

internal sealed record YahooChartError(
    [property: JsonPropertyName("code")] string? Code,
    [property: JsonPropertyName("description")] string? Description);

internal sealed record YahooChartMeta(
    [property: JsonPropertyName("symbol")] string? Symbol,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("longName")] string? LongName,
    [property: JsonPropertyName("shortName")] string? ShortName,
    [property: JsonPropertyName("fullExchangeName")] string? FullExchangeName,
    [property: JsonPropertyName("exchangeName")] string? ExchangeName,
    [property: JsonPropertyName("regularMarketPrice")] decimal? RegularMarketPrice,
    [property: JsonPropertyName("previousClose")] decimal? PreviousClose,
    [property: JsonPropertyName("chartPreviousClose")] decimal? ChartPreviousClose,
    [property: JsonPropertyName("regularMarketTime")] long? RegularMarketTime);
