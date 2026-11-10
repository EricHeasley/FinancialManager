using System.ComponentModel.DataAnnotations;

namespace FinancialManager.Web.Formatting;

/// <summary>
/// Presentation settings bound from the <c>Display</c> configuration section.
/// </summary>
public sealed class DisplayOptions
{
    public const string SectionName = "Display";

    /// <summary>
    /// Culture used for number, date and currency formatting (e.g. <c>en-US</c>, <c>en-GB</c>).
    /// The culture's currency is the base currency for all account balances.
    /// </summary>
    [Required]
    public string CultureName { get; set; } = string.Empty;
}
