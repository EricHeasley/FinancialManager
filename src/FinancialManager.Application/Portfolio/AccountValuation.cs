using FinancialManager.Application.Accounts.Models;

namespace FinancialManager.Application.Portfolio;

/// <summary>
/// An account valued at current market prices.
/// </summary>
public sealed record AccountValuation(AccountSummary Account, IReadOnlyList<HoldingValuation> Holdings)
{
    public decimal CashBalance => Account.CashBalance;

    public decimal HoldingsValue => Holdings.Sum(h => h.EffectiveValue);

    public decimal HoldingsCostBasis => Holdings.Sum(h => h.Holding.CostBasis);

    /// <summary>
    /// Cash plus holdings. For liability accounts this is the amount owed.
    /// </summary>
    public decimal TotalValue => CashBalance + HoldingsValue;

    public decimal? DayChange => Holdings.Any(h => h.DayChange.HasValue)
        ? Holdings.Sum(h => h.DayChange ?? decimal.Zero)
        : null;

    public bool HasUnpricedHoldings => Holdings.Any(h => !h.IsPriced);
}
