using FinancialManager.Application.Abstractions;

namespace FinancialManager.Infrastructure.Persistence;

internal sealed class EfUnitOfWork : IUnitOfWork
{
    private readonly FinancialDbContext _dbContext;

    public EfUnitOfWork(FinancialDbContext dbContext)
    {
        _dbContext = dbContext;
        Accounts = new AccountRepository(dbContext);
    }

    public IAccountRepository Accounts { get; }

    public Task SaveChangesAsync(CancellationToken cancellationToken) =>
        _dbContext.SaveChangesAsync(cancellationToken);

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
}
