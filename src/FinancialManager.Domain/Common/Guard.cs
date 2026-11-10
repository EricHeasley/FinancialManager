namespace FinancialManager.Domain.Common;

/// <summary>
/// Central argument checks used by domain entities to protect their invariants.
/// </summary>
internal static class Guard
{
    public static string AgainstNullOrWhiteSpace(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new DomainException($"{parameterName} is required.");
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static string? AgainstTooLong(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{parameterName} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }

    public static decimal AgainstNonPositive(decimal value, string parameterName)
    {
        if (value <= decimal.Zero)
        {
            throw new DomainException($"{parameterName} must be greater than zero.");
        }

        return value;
    }

    public static decimal AgainstNegative(decimal value, string parameterName)
    {
        if (value < decimal.Zero)
        {
            throw new DomainException($"{parameterName} cannot be negative.");
        }

        return value;
    }

    public static T AgainstUndefinedEnum<T>(T value, string parameterName)
        where T : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new DomainException($"{parameterName} has an unsupported value '{value}'.");
        }

        return value;
    }
}
