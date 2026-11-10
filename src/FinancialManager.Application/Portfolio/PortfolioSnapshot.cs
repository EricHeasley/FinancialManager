namespace FinancialManager.Application.Portfolio;

/// <summary>
/// Net-worth overview across all active accounts at a point in time.
/// </summary>
public sealed record PortfolioSnapshot(
    IReadOnlyList<AccountValuation> Accounts,
    IReadOnlyList<string> UnpricedSymbols,
    DateTime AsOfUtc)
{
    public decimal TotalAssets => Accounts.Where(a => !a.Account.IsLiability).Sum(a => a.TotalValue);

    public decimal TotalLiabilities => Accounts.Where(a => a.Account.IsLiability).Sum(a => a.TotalValue);

    public decimal NetWorth => TotalAssets - TotalLiabilities;

    public decimal InvestmentsValue => Accounts.Sum(a => a.HoldingsValue);

    public decimal CashValue => Accounts.Where(a => !a.Account.IsLiability).Sum(a => a.CashBalance);

    public decimal? DayChange => Accounts.Any(a => a.DayChange.HasValue)
        ? Accounts.Sum(a => a.DayChange ?? decimal.Zero)
        : null;
}
