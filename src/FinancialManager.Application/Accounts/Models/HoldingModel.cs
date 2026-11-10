namespace FinancialManager.Application.Accounts.Models;

public sealed record HoldingModel(
    Guid Id,
    string Symbol,
    decimal Quantity,
    decimal AverageCostPerShare,
    decimal CostBasis);
