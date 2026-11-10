using FinancialManager.Application.Abstractions;
using FinancialManager.Domain.Accounts;
using Microsoft.EntityFrameworkCore;

namespace FinancialManager.Infrastructure.Persistence;

internal sealed class AccountRepository : IAccountRepository
{
    private readonly FinancialDbContext _dbContext;

    public AccountRepository(FinancialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _dbContext.Accounts
            .Include(a => a.Transactions)
            .Include(a => a.Holdings)
            .AsSplitQuery()
            .SingleOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Account>> ListAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        var query = _dbContext.Accounts.Include(a => a.Holdings).AsQueryable();
        if (!includeArchived)
        {
            query = query.Where(a => !a.IsArchived);
        }

        return await query.AsSplitQuery().ToListAsync(cancellationToken);
    }

    public void Add(Account account) => _dbContext.Accounts.Add(account);

    public void Remove(Account account) => _dbContext.Accounts.Remove(account);
}
