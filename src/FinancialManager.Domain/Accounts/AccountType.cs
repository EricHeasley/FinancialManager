namespace FinancialManager.Domain.Accounts;

/// <summary>
/// The kind of financial account. Values are persisted, so existing numbers must never change.
/// </summary>
public enum AccountType
{
    Checking = 1,
    Savings = 2,
    Cash = 3,
    Brokerage = 4,
    Retirement = 5,
    CreditCard = 6,
    Loan = 7,
    Other = 99,
}
