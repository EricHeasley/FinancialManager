using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Quotes;

namespace FinancialManager.Application.Portfolio;

/// <summary>
/// Pure valuation logic that combines account read models with market quotes.
/// </summary>
public static class PortfolioValuator
{
    public static AccountValuation ValueAccount(
        AccountSummary account,
        IReadOnlyDictionary<string, StockQuote> quotes)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(quotes);

        var holdings = account.Holdings
            .Select(h => new HoldingValuation(h, quotes.GetValueOrDefault(h.Symbol)))
            .ToList();

        return new AccountValuation(account, holdings);
    }

    public static PortfolioSnapshot ValuePortfolio(
        IEnumerable<AccountSummary> accounts,
        IReadOnlyDictionary<string, StockQuote> quotes,
        DateTime asOfUtc)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(quotes);

        var valuations = accounts.Select(a => ValueAccount(a, quotes)).ToList();
        var unpricedSymbols = valuations
            .SelectMany(v => v.Holdings)
            .Where(h => !h.IsPriced)
            .Select(h => h.Holding.Symbol)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();

        return new PortfolioSnapshot(valuations, unpricedSymbols, asOfUtc);
    }
}
