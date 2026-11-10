using FinancialManager.Domain.Accounts;

namespace FinancialManager.Application.Accounts.Models;

/// <summary>
/// Read model describing an account for list views.
/// </summary>
public sealed record AccountSummary(
    Guid Id,
    string Name,
    AccountType Type,
    string? Institution,
    decimal CashBalance,
    bool IsLiability,
    bool SupportsHoldings,
    bool IsArchived,
    IReadOnlyList<HoldingModel> Holdings,
    DateTime UpdatedUtc);
