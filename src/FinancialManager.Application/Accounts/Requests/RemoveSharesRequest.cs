using System.ComponentModel.DataAnnotations;
using FinancialManager.Application.Common;
using FinancialManager.Domain.Common;

namespace FinancialManager.Application.Accounts.Requests;

public sealed class RemoveSharesRequest
{
    [Required(ErrorMessage = "Symbol is required.")]
    [StringLength(DomainLimits.SymbolMaxLength, ErrorMessage = "Symbol is too long.")]
    [RegularExpression(ValidationPatterns.TickerSymbol, ErrorMessage = "Symbols may contain letters, numbers, '.', '-', '^' and '='.")]
    public string Symbol { get; set; } = string.Empty;

    [Range(typeof(decimal), ValidationRanges.MinimumPositiveQuantity, ValidationRanges.MaximumAmount, ErrorMessage = "Quantity must be greater than zero.")]
    public decimal Quantity { get; set; }
}
