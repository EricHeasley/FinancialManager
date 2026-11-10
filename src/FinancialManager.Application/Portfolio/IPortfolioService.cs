using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Common;

namespace FinancialManager.Application.Portfolio;

/// <summary>
/// Values accounts and the overall portfolio using live market prices.
/// </summary>
public interface IPortfolioService
{
    Task<PortfolioSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);

    Task<Result<AccountValuation>> GetAccountValuationAsync(Guid accountId, CancellationToken cancellationToken);

    /// <summary>Values an account that has already been loaded.</summary>
    Task<AccountValuation> ValueAccountAsync(AccountSummary account, CancellationToken cancellationToken);
}
