namespace FinancialManager.Application.Common;

/// <summary>
/// Category of a failed operation, used by the UI to decide how to present the error.
/// </summary>
public enum ErrorType
{
    Validation = 1,
    NotFound = 2,
    ExternalService = 3,
}
