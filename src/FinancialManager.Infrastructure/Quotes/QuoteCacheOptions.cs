using System.ComponentModel.DataAnnotations;

namespace FinancialManager.Infrastructure.Quotes;

/// <summary>
/// Settings for the in-memory quote cache. Bound from the <c>Quotes:Cache</c> section.
/// </summary>
public sealed class QuoteCacheOptions
{
    public const string SectionName = "Quotes:Cache";

    /// <summary>How long a retrieved quote is reused before the provider is called again.</summary>
    [Range(0, 3600)]
    public int DurationSeconds { get; set; }
}
