namespace FinancialManager.Domain.Accounts;

/// <summary>
/// Direction of a cash movement. Values are persisted, so existing numbers must never change.
/// </summary>
public enum TransactionType
{
    /// <summary>Money coming in. For a liability account this is a payment that reduces the amount owed.</summary>
    Deposit = 1,

    /// <summary>Money going out. For a liability account this is a charge that increases the amount owed.</summary>
    Withdrawal = 2,
}
