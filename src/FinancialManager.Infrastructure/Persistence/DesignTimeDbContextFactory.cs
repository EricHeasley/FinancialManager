using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinancialManager.Infrastructure.Persistence;

/// <summary>
/// Used only by the <c>dotnet ef</c> tooling to create migrations. Not used at runtime.
/// </summary>
internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<FinancialDbContext>
{
    private const string DesignTimeConnectionString = "Data Source=design-time.db";

    public FinancialDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FinancialDbContext>()
            .UseSqlite(DesignTimeConnectionString)
            .Options;

        return new FinancialDbContext(options);
    }
}
