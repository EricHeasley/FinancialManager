using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialManager.Infrastructure.Persistence;

public static partial class DatabaseInitializer
{
    /// <summary>
    /// Ensures the database directory exists and applies pending migrations when enabled.
    /// </summary>
    public static async Task InitializeDatabaseAsync(this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(services);

        var options = services.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        if (!options.ApplyMigrationsOnStartup)
        {
            return;
        }

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer));
        var dbContextFactory = services.GetRequiredService<IDbContextFactory<FinancialDbContext>>();
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        EnsureDataDirectoryExists(dbContext.Database.GetConnectionString());

        LogApplyingMigrations(logger);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static void EnsureDataDirectoryExists(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return;
        }

        var dataSource = new SqliteConnectionStringBuilder(connectionString).DataSource;
        var directory = Path.GetDirectoryName(Path.GetFullPath(dataSource));
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    [LoggerMessage(EventId = 3001, Level = LogLevel.Information, Message = "Applying database migrations.")]
    private static partial void LogApplyingMigrations(ILogger logger);
}
