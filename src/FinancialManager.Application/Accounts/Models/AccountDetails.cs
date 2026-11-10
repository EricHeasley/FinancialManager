namespace FinancialManager.Application.Accounts.Models;

/// <summary>
/// Read model describing a single account and its full activity.
/// </summary>
public sealed record AccountDetails(
    AccountSummary Summary,
    string? Notes,
    decimal OpeningBalance,
    DateTime CreatedUtc,
    IReadOnlyList<TransactionModel> Transactions);
