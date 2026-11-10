using System.ComponentModel.DataAnnotations;

namespace FinancialManager.Application.Quotes;

/// <summary>
/// Settings that control how quotes are requested. Bound from the <c>Quotes</c> configuration section.
/// </summary>
public sealed class QuoteOptions
{
    public const string SectionName = "Quotes";

    /// <summary>Maximum number of concurrent requests sent to the provider when pricing many symbols.</summary>
    [Range(1, 32)]
    public int MaxConcurrentRequests { get; set; } = 4;
}
