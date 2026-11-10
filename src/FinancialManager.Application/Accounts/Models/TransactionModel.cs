using FinancialManager.Domain.Accounts;

namespace FinancialManager.Application.Accounts.Models;

public sealed record TransactionModel(
    Guid Id,
    DateOnly Date,
    TransactionType Type,
    decimal Amount,
    string Description,
    DateTime CreatedUtc);
