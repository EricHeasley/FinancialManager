using System.ComponentModel.DataAnnotations;

namespace FinancialManager.Application.Common;

/// <summary>
/// Validates request models using their data-annotation attributes. The same attributes drive
/// client-side form validation in the UI, so rules are defined once.
/// </summary>
public static class RequestValidator
{
    public static Error? Validate(object request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);
        if (Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            return null;
        }

        var message = string.Join(" ", results.Select(r => r.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)));
        return Error.Validation(message);
    }
}
