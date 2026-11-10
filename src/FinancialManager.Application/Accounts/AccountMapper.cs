using FinancialManager.Application.Accounts.Models;
using FinancialManager.Domain.Accounts;

namespace FinancialManager.Application.Accounts;

/// <summary>
/// Maps domain aggregates to read models so domain entities never leave the application layer.
/// </summary>
internal static class AccountMapper
{
    public static AccountSummary ToSummary(Account account) => new(
        account.Id,
        account.Name,
        account.Type,
        account.Institution,
        account.CashBalance,
        account.IsLiability,
        account.SupportsHoldings,
        account.IsArchived,
        account.Holdings
            .OrderBy(h => h.Symbol, StringComparer.Ordinal)
            .Select(ToModel)
            .ToList(),
        account.UpdatedUtc);

    public static AccountDetails ToDetails(Account account) => new(
        ToSummary(account),
        account.Notes,
        account.OpeningBalance,
        account.CreatedUtc,
        account.Transactions
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedUtc)
            .Select(ToModel)
            .ToList());

    private static HoldingModel ToModel(Holding holding) => new(
        holding.Id,
        holding.Symbol,
        holding.Quantity,
        holding.AverageCostPerShare,
        holding.CostBasis);

    private static TransactionModel ToModel(Transaction transaction) => new(
        transaction.Id,
        transaction.Date,
        transaction.Type,
        transaction.Amount,
        transaction.Description,
        transaction.CreatedUtc);
}
