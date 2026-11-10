using FinancialManager.Domain.Common;

namespace FinancialManager.Domain.Accounts;

/// <summary>
/// A position in a single security (stock, ETF, fund) held within an investment <see cref="Account"/>.
/// </summary>
public sealed class Holding
{
    internal Holding(Guid accountId, string symbol, decimal quantity, decimal averageCostPerShare, DateTime createdUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Symbol = NormalizeSymbol(symbol);
        Quantity = Guard.AgainstNonPositive(quantity, nameof(quantity));
        AverageCostPerShare = Guard.AgainstNegative(averageCostPerShare, nameof(averageCostPerShare));
        CreatedUtc = createdUtc;
        UpdatedUtc = createdUtc;
    }

    // Required by the persistence layer for materialization.
    private Holding()
    {
        Symbol = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    /// <summary>Upper-case ticker symbol, e.g. <c>MSFT</c>.</summary>
    public string Symbol { get; private set; }

    public decimal Quantity { get; private set; }

    public decimal AverageCostPerShare { get; private set; }

    public decimal CostBasis => Quantity * AverageCostPerShare;

    public DateTime CreatedUtc { get; private set; }

    public DateTime UpdatedUtc { get; private set; }

    // "G29" drops insignificant trailing zeros that the data store may preserve (e.g. 5.000000 → 5).
    private static string FormatQuantity(decimal quantity) =>
        quantity.ToString("G29", System.Globalization.CultureInfo.CurrentCulture);

    /// <summary>
    /// Normalizes user-entered ticker symbols so they compare consistently.
    /// </summary>
    public static string NormalizeSymbol(string? symbol) =>
        Guard.AgainstNullOrWhiteSpace(symbol, nameof(symbol), DomainLimits.SymbolMaxLength)
            .ToUpperInvariant();

    /// <summary>
    /// Adds shares to the position and recalculates the weighted average cost.
    /// </summary>
    internal void AddShares(decimal quantity, decimal pricePerShare, DateTime utcNow)
    {
        Guard.AgainstNonPositive(quantity, nameof(quantity));
        Guard.AgainstNegative(pricePerShare, nameof(pricePerShare));

        var totalCost = CostBasis + (quantity * pricePerShare);
        Quantity += quantity;
        AverageCostPerShare = totalCost / Quantity;
        UpdatedUtc = utcNow;
    }

    /// <summary>
    /// Removes shares from the position. The average cost of the remaining shares is unchanged.
    /// </summary>
    internal void RemoveShares(decimal quantity, DateTime utcNow)
    {
        Guard.AgainstNonPositive(quantity, nameof(quantity));
        if (quantity > Quantity)
        {
            throw new DomainException(
                $"Cannot remove {FormatQuantity(quantity)} shares of {Symbol}; only {FormatQuantity(Quantity)} are held.");
        }

        Quantity -= quantity;
        UpdatedUtc = utcNow;
    }
}
