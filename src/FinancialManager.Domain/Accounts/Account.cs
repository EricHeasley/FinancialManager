using FinancialManager.Domain.Common;

namespace FinancialManager.Domain.Accounts;

/// <summary>
/// Aggregate root representing a financial account (bank, card, loan or investment account).
/// All changes to transactions and holdings go through this type so balances stay consistent.
/// </summary>
public sealed class Account
{
    private readonly List<Transaction> _transactions = [];
    private readonly List<Holding> _holdings = [];

    public Account(
        string name,
        AccountType type,
        string? institution,
        string? notes,
        decimal openingBalance,
        DateTime createdUtc)
    {
        Id = Guid.NewGuid();
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), DomainLimits.AccountNameMaxLength);
        Type = Guard.AgainstUndefinedEnum(type, nameof(type));
        Institution = Guard.AgainstTooLong(institution, nameof(institution), DomainLimits.InstitutionMaxLength);
        Notes = Guard.AgainstTooLong(notes, nameof(notes), DomainLimits.NotesMaxLength);
        OpeningBalance = openingBalance;
        CashBalance = openingBalance;
        CreatedUtc = createdUtc;
        UpdatedUtc = createdUtc;
    }

    // Required by the persistence layer for materialization.
    private Account()
    {
        Name = string.Empty;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public AccountType Type { get; private set; }

    public string? Institution { get; private set; }

    public string? Notes { get; private set; }

    /// <summary>The balance entered when the account was created.</summary>
    public decimal OpeningBalance { get; private set; }

    /// <summary>
    /// Current cash balance. For liability accounts this is the amount owed.
    /// </summary>
    public decimal CashBalance { get; private set; }

    public bool IsArchived { get; private set; }

    public DateTime CreatedUtc { get; private set; }

    public DateTime UpdatedUtc { get; private set; }

    public bool IsLiability => Type.IsLiability();

    public bool SupportsHoldings => Type.SupportsHoldings();

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    public IReadOnlyCollection<Holding> Holdings => _holdings.AsReadOnly();

    public void UpdateDetails(string name, string? institution, string? notes, DateTime utcNow)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), DomainLimits.AccountNameMaxLength);
        Institution = Guard.AgainstTooLong(institution, nameof(institution), DomainLimits.InstitutionMaxLength);
        Notes = Guard.AgainstTooLong(notes, nameof(notes), DomainLimits.NotesMaxLength);
        UpdatedUtc = utcNow;
    }

    public void Archive(DateTime utcNow)
    {
        IsArchived = true;
        UpdatedUtc = utcNow;
    }

    public void Restore(DateTime utcNow)
    {
        IsArchived = false;
        UpdatedUtc = utcNow;
    }

    /// <summary>
    /// Records a cash movement and applies it to <see cref="CashBalance"/>.
    /// </summary>
    public Transaction RecordTransaction(
        DateOnly date,
        TransactionType type,
        decimal amount,
        string description,
        DateTime utcNow)
    {
        EnsureNotArchived();

        var transaction = new Transaction(Id, date, type, amount, description, utcNow);
        CashBalance += GetBalanceEffect(transaction.Type, transaction.Amount);
        _transactions.Add(transaction);
        UpdatedUtc = utcNow;
        return transaction;
    }

    /// <summary>
    /// Removes a previously recorded transaction and reverses its effect on the balance.
    /// </summary>
    public void RemoveTransaction(Guid transactionId, DateTime utcNow)
    {
        EnsureNotArchived();

        var transaction = _transactions.Find(t => t.Id == transactionId)
            ?? throw new DomainException("The transaction does not exist on this account.");

        CashBalance -= GetBalanceEffect(transaction.Type, transaction.Amount);
        _transactions.Remove(transaction);
        UpdatedUtc = utcNow;
    }

    /// <summary>
    /// Adds shares of a security. If the symbol is already held, the shares are merged into the
    /// existing position using a weighted average cost.
    /// </summary>
    public Holding AddShares(string symbol, decimal quantity, decimal pricePerShare, DateTime utcNow)
    {
        EnsureNotArchived();
        EnsureSupportsHoldings();

        var normalizedSymbol = Holding.NormalizeSymbol(symbol);
        var existing = FindHolding(normalizedSymbol);
        if (existing is not null)
        {
            existing.AddShares(quantity, pricePerShare, utcNow);
            UpdatedUtc = utcNow;
            return existing;
        }

        var holding = new Holding(Id, normalizedSymbol, quantity, pricePerShare, utcNow);
        _holdings.Add(holding);
        UpdatedUtc = utcNow;
        return holding;
    }

    /// <summary>
    /// Removes shares of a security. The position is deleted once no shares remain.
    /// </summary>
    public void RemoveShares(string symbol, decimal quantity, DateTime utcNow)
    {
        EnsureNotArchived();
        EnsureSupportsHoldings();

        var normalizedSymbol = Holding.NormalizeSymbol(symbol);
        var holding = FindHolding(normalizedSymbol)
            ?? throw new DomainException($"No position in {normalizedSymbol} exists on this account.");

        holding.RemoveShares(quantity, utcNow);
        if (holding.Quantity == decimal.Zero)
        {
            _holdings.Remove(holding);
        }

        UpdatedUtc = utcNow;
    }

    private Holding? FindHolding(string normalizedSymbol) =>
        _holdings.Find(h => string.Equals(h.Symbol, normalizedSymbol, StringComparison.Ordinal));

    private decimal GetBalanceEffect(TransactionType type, decimal amount)
    {
        // Asset accounts: deposits increase the balance.
        // Liability accounts: the balance is the amount owed, so deposits (payments) decrease it.
        var isIncrease = (type == TransactionType.Deposit) ^ IsLiability;
        return isIncrease ? amount : -amount;
    }

    private void EnsureNotArchived()
    {
        if (IsArchived)
        {
            throw new DomainException("Archived accounts cannot be modified. Restore the account first.");
        }
    }

    private void EnsureSupportsHoldings()
    {
        if (!SupportsHoldings)
        {
            throw new DomainException($"{Type} accounts cannot hold securities.");
        }
    }
}
