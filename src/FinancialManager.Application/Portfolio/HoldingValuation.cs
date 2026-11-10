using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Quotes;

namespace FinancialManager.Application.Portfolio;

/// <summary>
/// A holding valued at the latest available market price.
/// </summary>
public sealed record HoldingValuation(HoldingModel Holding, StockQuote? Quote)
{
    public bool IsPriced => Quote is not null;

    public decimal? MarketValue => Quote is null ? null : Holding.Quantity * Quote.Price;

    /// <summary>
    /// Market value when a price is available, otherwise the cost basis so totals remain meaningful.
    /// </summary>
    public decimal EffectiveValue => MarketValue ?? Holding.CostBasis;

    public decimal? UnrealizedGain => MarketValue - Holding.CostBasis;

    public decimal? UnrealizedGainPercent => UnrealizedGain is { } gain && Holding.CostBasis != decimal.Zero
        ? gain / Holding.CostBasis
        : null;

    public decimal? DayChange => Quote?.Change * Holding.Quantity;
}
