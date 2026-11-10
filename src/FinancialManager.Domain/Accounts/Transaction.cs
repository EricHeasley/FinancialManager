using FinancialManager.Domain.Common;

namespace FinancialManager.Domain.Accounts;

/// <summary>
/// An immutable cash movement recorded against an <see cref="Account"/>.
/// </summary>
public sealed class Transaction
{
    internal Transaction(
        Guid accountId,
        DateOnly date,
        TransactionType type,
        decimal amount,
        string description,
        DateTime createdUtc)
    {
        Id = Guid.NewGuid();
        AccountId = accountId;
        Date = date;
        Type = Guard.AgainstUndefinedEnum(type, nameof(type));
        Amount = Guard.AgainstNonPositive(amount, nameof(amount));
        Description = Guard.AgainstNullOrWhiteSpace(description, nameof(description), DomainLimits.TransactionDescriptionMaxLength);
        CreatedUtc = createdUtc;
    }

    // Required by the persistence layer for materialization.
    private Transaction()
    {
        Description = string.Empty;
    }

    public Guid Id { get; private set; }

    public Guid AccountId { get; private set; }

    public DateOnly Date { get; private set; }

    public TransactionType Type { get; private set; }

    /// <summary>Always positive; the direction is expressed by <see cref="Type"/>.</summary>
    public decimal Amount { get; private set; }

    public string Description { get; private set; }

    public DateTime CreatedUtc { get; private set; }
}
