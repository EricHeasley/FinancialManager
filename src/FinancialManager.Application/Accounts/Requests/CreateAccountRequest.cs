using System.ComponentModel.DataAnnotations;
using FinancialManager.Domain.Accounts;
using FinancialManager.Domain.Common;

namespace FinancialManager.Application.Accounts.Requests;

public sealed class CreateAccountRequest
{
    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(DomainLimits.AccountNameMaxLength, ErrorMessage = "Account name is too long.")]
    public string Name { get; set; } = string.Empty;

    [EnumDataType(typeof(AccountType), ErrorMessage = "Select a valid account type.")]
    public AccountType Type { get; set; } = AccountType.Checking;

    [StringLength(DomainLimits.InstitutionMaxLength, ErrorMessage = "Institution name is too long.")]
    public string? Institution { get; set; }

    [StringLength(DomainLimits.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; set; }

    /// <summary>
    /// Starting cash balance. For credit cards and loans, enter the amount currently owed.
    /// </summary>
    public decimal OpeningBalance { get; set; }
}
