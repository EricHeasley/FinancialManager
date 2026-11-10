using FinancialManager.Application.Accounts;
using FinancialManager.Application.Portfolio;
using FinancialManager.Application.Quotes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FinancialManager.Application;

public static class DependencyInjection
{
    /// <summary>
    /// Registers application-layer use cases.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddOptions<QuoteOptions>()
            .Bind(configuration.GetSection(QuoteOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IPortfolioService, PortfolioService>();

        return services;
    }
}
