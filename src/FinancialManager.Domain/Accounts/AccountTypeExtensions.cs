namespace FinancialManager.Domain.Accounts;

/// <summary>
/// Business classification rules for <see cref="AccountType"/>.
/// </summary>
public static class AccountTypeExtensions
{
    /// <summary>
    /// Liability accounts store the amount owed; their balance reduces net worth.
    /// </summary>
    public static bool IsLiability(this AccountType accountType) =>
        accountType is AccountType.CreditCard or AccountType.Loan;

    /// <summary>
    /// Investment accounts may hold securities in addition to a cash balance.
    /// </summary>
    public static bool SupportsHoldings(this AccountType accountType) =>
        accountType is AccountType.Brokerage or AccountType.Retirement;
}
