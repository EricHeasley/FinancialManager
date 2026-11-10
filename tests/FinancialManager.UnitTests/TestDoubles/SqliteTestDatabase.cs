using FinancialManager.Application.Abstractions;
using FinancialManager.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FinancialManager.UnitTests.TestDoubles;

/// <summary>
/// An in-memory SQLite database with the real schema, kept alive for the lifetime of the test.
/// Exercises the actual EF Core mappings rather than a fake provider.
/// </summary>
internal sealed class SqliteTestDatabase : IDisposable, IDbContextFactory<FinancialDbContext>
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<FinancialDbContext> _options;

    public SqliteTestDatabase()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<FinancialDbContext>().UseSqlite(_connection).Options;

        using var context = CreateDbContext();
        context.Database.Migrate();
    }

    public IUnitOfWorkFactory UnitOfWorkFactory => new EfUnitOfWorkFactory(this);

    public FinancialDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
