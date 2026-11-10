using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Accounts.Requests;
using FinancialManager.Application.Common;

namespace FinancialManager.Application.Accounts;

/// <summary>
/// Use cases for managing accounts, their transactions and their holdings.
/// </summary>
public interface IAccountService
{
    Task<IReadOnlyList<AccountSummary>> GetAccountsAsync(bool includeArchived, CancellationToken cancellationToken);

    Task<Result<AccountDetails>> GetAccountAsync(Guid accountId, CancellationToken cancellationToken);

    Task<Result<Guid>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken);

    Task<Result> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken);

    Task<Result> ArchiveAccountAsync(Guid accountId, CancellationToken cancellationToken);

    Task<Result> RestoreAccountAsync(Guid accountId, CancellationToken cancellationToken);

    Task<Result> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken);

    Task<Result> RecordTransactionAsync(Guid accountId, RecordTransactionRequest request, CancellationToken cancellationToken);

    Task<Result> RemoveTransactionAsync(Guid accountId, Guid transactionId, CancellationToken cancellationToken);

    Task<Result> AddSharesAsync(Guid accountId, AddSharesRequest request, CancellationToken cancellationToken);

    Task<Result> RemoveSharesAsync(Guid accountId, RemoveSharesRequest request, CancellationToken cancellationToken);
}
