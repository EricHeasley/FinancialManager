using FinancialManager.Domain.Accounts;

namespace FinancialManager.Web.Formatting;

/// <summary>
/// User-facing names for domain enumerations.
/// </summary>
public static class EnumDisplayExtensions
{
    public static string ToDisplayName(this AccountType accountType) => accountType switch
    {
        AccountType.Checking => "Checking",
        AccountType.Savings => "Savings",
        AccountType.Cash => "Cash",
        AccountType.Brokerage => "Brokerage",
        AccountType.Retirement => "Retirement",
        AccountType.CreditCard => "Credit card",
        AccountType.Loan => "Loan",
        AccountType.Other => "Other",
        _ => accountType.ToString(),
    };

    /// <summary>
    /// Transaction labels depend on the account: a deposit to a credit card is a payment.
    /// </summary>
    public static string ToDisplayName(this TransactionType transactionType, bool isLiability) => transactionType switch
    {
        TransactionType.Deposit => isLiability ? "Payment" : "Deposit",
        TransactionType.Withdrawal => isLiability ? "Charge" : "Withdrawal",
        _ => transactionType.ToString(),
    };

    /// <summary>CSS modifier used for the account type badge.</summary>
    public static string ToBadgeClass(this AccountType accountType) =>
        accountType.IsLiability() ? "badge-liability"
        : accountType.SupportsHoldings() ? "badge-investment"
        : "badge-cash";
}
