using FinancialManager.Application.Common;

namespace FinancialManager.Application.Quotes;

/// <summary>
/// Looks up current market prices for securities.
/// </summary>
public interface IQuoteService
{
    Task<Result<StockQuote>> GetQuoteAsync(QuoteLookupRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Fetches quotes for several symbols. Symbols that cannot be priced are omitted from the result.
    /// </summary>
    Task<IReadOnlyDictionary<string, StockQuote>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken);
}
