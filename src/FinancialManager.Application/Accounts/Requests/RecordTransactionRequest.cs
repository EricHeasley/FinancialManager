using System.ComponentModel.DataAnnotations;
using FinancialManager.Domain.Accounts;
using FinancialManager.Application.Common;
using FinancialManager.Domain.Common;

namespace FinancialManager.Application.Accounts.Requests;

public sealed class RecordTransactionRequest
{
    [Required(ErrorMessage = "Date is required.")]
    public DateOnly? Date { get; set; }

    [EnumDataType(typeof(TransactionType), ErrorMessage = "Select a valid transaction type.")]
    public TransactionType Type { get; set; } = TransactionType.Deposit;

    [Range(typeof(decimal), ValidationRanges.MinimumPositiveAmount, ValidationRanges.MaximumAmount, ErrorMessage = "Amount must be greater than zero.")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(DomainLimits.TransactionDescriptionMaxLength, ErrorMessage = "Description is too long.")]
    public string Description { get; set; } = string.Empty;
}
