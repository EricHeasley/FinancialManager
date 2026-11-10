using System.ComponentModel.DataAnnotations;
using FinancialManager.Domain.Common;

namespace FinancialManager.Application.Accounts.Requests;

public sealed class UpdateAccountRequest
{
    [Required(ErrorMessage = "Account name is required.")]
    [StringLength(DomainLimits.AccountNameMaxLength, ErrorMessage = "Account name is too long.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(DomainLimits.InstitutionMaxLength, ErrorMessage = "Institution name is too long.")]
    public string? Institution { get; set; }

    [StringLength(DomainLimits.NotesMaxLength, ErrorMessage = "Notes are too long.")]
    public string? Notes { get; set; }
}
