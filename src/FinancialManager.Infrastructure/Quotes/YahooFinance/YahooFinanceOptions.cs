using System.ComponentModel.DataAnnotations;

namespace FinancialManager.Infrastructure.Quotes.YahooFinance;

/// <summary>
/// Connection settings for the Yahoo Finance chart API. Bound from the <c>Quotes:YahooFinance</c> section.
/// </summary>
public sealed class YahooFinanceOptions
{
    public const string SectionName = "Quotes:YahooFinance";

    /// <summary>Base address of the API, e.g. <c>https://query1.finance.yahoo.com/</c>.</summary>
    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Relative path of the chart endpoint. The symbol is appended to this path.</summary>
    [Required]
    public string ChartPath { get; set; } = string.Empty;

    /// <summary>Query string appended to chart requests, e.g. <c>interval=1d&amp;range=1d</c>.</summary>
    [Required]
    public string ChartQuery { get; set; } = string.Empty;

    /// <summary>User-Agent header sent with each request. The API rejects requests without one.</summary>
    [Required]
    public string UserAgent { get; set; } = string.Empty;
}
