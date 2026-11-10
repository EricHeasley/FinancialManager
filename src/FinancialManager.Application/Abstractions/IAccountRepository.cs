using FinancialManager.Domain.Accounts;

namespace FinancialManager.Application.Abstractions;

/// <summary>
/// Persistence operations for the <see cref="Account"/> aggregate.
/// Returned aggregates are tracked by the owning <see cref="IUnitOfWork"/>.
/// </summary>
public interface IAccountRepository
{
    /// <summary>Loads an account including its transactions and holdings.</summary>
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Loads all accounts including their holdings (transactions are not loaded).</summary>
    Task<IReadOnlyList<Account>> ListAsync(bool includeArchived, CancellationToken cancellationToken);

    void Add(Account account);

    void Remove(Account account);
}
