using FinancialManager.Application.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace FinancialManager.Infrastructure.Persistence;

internal sealed class EfUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IDbContextFactory<FinancialDbContext> _dbContextFactory;

    public EfUnitOfWorkFactory(IDbContextFactory<FinancialDbContext> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public IUnitOfWork Create() => new EfUnitOfWork(_dbContextFactory.CreateDbContext());
}
