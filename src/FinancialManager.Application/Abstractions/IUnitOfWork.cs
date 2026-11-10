namespace FinancialManager.Application.Abstractions;

/// <summary>
/// A short-lived transactional scope over the data store. Create one per operation via
/// <see cref="IUnitOfWorkFactory"/> and dispose it when the operation completes.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    IAccountRepository Accounts { get; }

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
