namespace FinancialManager.Domain.Common;

/// <summary>
/// Raised when an operation would violate a business invariant of the domain model.
/// </summary>
public sealed class DomainException : Exception
{
    public DomainException()
    {
    }

    public DomainException(string message)
        : base(message)
    {
    }

    public DomainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
