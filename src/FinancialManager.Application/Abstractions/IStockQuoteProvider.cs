using FinancialManager.Application.Quotes;

namespace FinancialManager.Application.Abstractions;

/// <summary>
/// Retrieves current market prices from an external market-data source.
/// </summary>
public interface IStockQuoteProvider
{
    /// <summary>
    /// Gets the latest quote for a normalized (upper-case) symbol.
    /// </summary>
    /// <returns>The quote, or <c>null</c> if the symbol is not recognised by the provider.</returns>
    /// <exception cref="QuoteProviderException">The provider could not be reached or returned invalid data.</exception>
    Task<StockQuote?> GetQuoteAsync(string symbol, CancellationToken cancellationToken);
}
