using FinancialManager.Application.Abstractions;
using FinancialManager.Infrastructure.Persistence;
using FinancialManager.Infrastructure.Quotes;
using FinancialManager.Infrastructure.Quotes.YahooFinance;
using FinancialManager.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FinancialManager.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Configuration section bound to <c>HttpStandardResilienceOptions</c> (retry, timeouts, circuit breaker)
    /// for calls to the quote provider.
    /// </summary>
    public const string QuoteResilienceSectionName = "Quotes:Resilience";

    /// <summary>
    /// Registers persistence, market-data and system services.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IClock, SystemClock>();

        AddPersistence(services, configuration);
        AddQuotes(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddDbContextFactory<FinancialDbContext>((serviceProvider, options) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var connectionString = configuration.GetConnectionString(databaseOptions.ConnectionStringName)
                ?? throw new InvalidOperationException(
                    $"Connection string '{databaseOptions.ConnectionStringName}' is not configured.");

            options.UseSqlite(connectionString);
        });

        services.AddSingleton<IUnitOfWorkFactory, EfUnitOfWorkFactory>();
    }

    private static void AddQuotes(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();

        services.AddOptions<QuoteCacheOptions>()
            .Bind(configuration.GetSection(QuoteCacheOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<YahooFinanceOptions>()
            .Bind(configuration.GetSection(YahooFinanceOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddHttpClient<YahooFinanceQuoteProvider>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<YahooFinanceOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.UserAgent.ParseAdd(options.UserAgent);
            })
            .AddStandardResilienceHandler(configuration.GetSection(QuoteResilienceSectionName));

        services.AddTransient<IStockQuoteProvider>(serviceProvider => new CachingStockQuoteProvider(
            serviceProvider.GetRequiredService<YahooFinanceQuoteProvider>(),
            serviceProvider.GetRequiredService<IMemoryCache>(),
            serviceProvider.GetRequiredService<IOptions<QuoteCacheOptions>>()));
    }
}
