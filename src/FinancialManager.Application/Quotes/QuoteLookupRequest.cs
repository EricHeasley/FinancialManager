using FinancialManager.Application.Common;
using System.ComponentModel.DataAnnotations;
using FinancialManager.Domain.Common;

namespace FinancialManager.Application.Quotes;

public sealed class QuoteLookupRequest
{
    [Required(ErrorMessage = "Enter a ticker symbol.")]
    [StringLength(DomainLimits.SymbolMaxLength, ErrorMessage = "Symbol is too long.")]
    [RegularExpression(ValidationPatterns.TickerSymbol, ErrorMessage = "Symbols may contain letters, numbers, '.', '-', '^' and '='.")]
    public string Symbol { get; set; } = string.Empty;
}
