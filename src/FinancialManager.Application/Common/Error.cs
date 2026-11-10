namespace FinancialManager.Application.Common;

/// <summary>
/// Describes why an operation failed.
/// </summary>
public sealed record Error(ErrorType Type, string Message)
{
    public static Error Validation(string message) => new(ErrorType.Validation, message);

    public static Error NotFound(string message) => new(ErrorType.NotFound, message);

    public static Error ExternalService(string message) => new(ErrorType.ExternalService, message);
}
