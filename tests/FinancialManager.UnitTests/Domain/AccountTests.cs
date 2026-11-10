using FinancialManager.Domain.Accounts;
using FinancialManager.Domain.Common;

namespace FinancialManager.UnitTests.Domain;

public sealed class AccountTests
{
    private static readonly DateTime Now = new(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);

    [Fact]
    public void Constructor_SetsCashBalanceToOpeningBalance()
    {
        var account = CreateAccount(AccountType.Checking, openingBalance: 1_000m);

        account.CashBalance.Should().Be(1_000m);
        account.OpeningBalance.Should().Be(1_000m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithoutName_Throws(string name)
    {
        var act = () => new Account(name, AccountType.Checking, null, null, 0m, Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Constructor_TrimsTextFields()
    {
        var account = new Account("  Savings  ", AccountType.Savings, "  Bank  ", "   ", 0m, Now);

        account.Name.Should().Be("Savings");
        account.Institution.Should().Be("Bank");
        account.Notes.Should().BeNull();
    }

    [Theory]
    [InlineData(TransactionType.Deposit, 1_250)]
    [InlineData(TransactionType.Withdrawal, 750)]
    public void RecordTransaction_OnAssetAccount_AdjustsBalance(TransactionType type, decimal expectedBalance)
    {
        var account = CreateAccount(AccountType.Checking, openingBalance: 1_000m);

        account.RecordTransaction(Today, type, 250m, "Test", Now);

        account.CashBalance.Should().Be(expectedBalance);
        account.Transactions.Should().ContainSingle();
    }

    [Theory]
    [InlineData(TransactionType.Deposit, 300)]
    [InlineData(TransactionType.Withdrawal, 700)]
    public void RecordTransaction_OnLiabilityAccount_TreatsDepositAsPayment(TransactionType type, decimal expectedOwed)
    {
        var account = CreateAccount(AccountType.CreditCard, openingBalance: 500m);

        account.RecordTransaction(Today, type, 200m, "Test", Now);

        account.CashBalance.Should().Be(expectedOwed);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void RecordTransaction_WithNonPositiveAmount_Throws(decimal amount)
    {
        var account = CreateAccount(AccountType.Checking);

        var act = () => account.RecordTransaction(Today, TransactionType.Deposit, amount, "Test", Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveTransaction_ReversesBalanceEffect()
    {
        var account = CreateAccount(AccountType.Checking, openingBalance: 100m);
        var transaction = account.RecordTransaction(Today, TransactionType.Withdrawal, 40m, "Coffee", Now);

        account.RemoveTransaction(transaction.Id, Now);

        account.CashBalance.Should().Be(100m);
        account.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void RemoveTransaction_WhenUnknown_Throws()
    {
        var account = CreateAccount(AccountType.Checking);

        var act = () => account.RemoveTransaction(Guid.NewGuid(), Now);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddShares_NormalizesSymbolAndMergesWithWeightedAverageCost()
    {
        var account = CreateAccount(AccountType.Brokerage);

        account.AddShares(" aapl ", 10m, 100m, Now);
        account.AddShares("AAPL", 10m, 200m, Now);

        var holding = account.Holdings.Should().ContainSingle().Subject;
        holding.Symbol.Should().Be("AAPL");
        holding.Quantity.Should().Be(20m);
        holding.AverageCostPerShare.Should().Be(150m);
        holding.CostBasis.Should().Be(3_000m);
    }

    [Fact]
    public void AddShares_OnNonInvestmentAccount_Throws()
    {
        var account = CreateAccount(AccountType.Checking);

        var act = () => account.AddShares("AAPL", 1m, 100m, Now);

        act.Should().Throw<DomainException>().WithMessage("*cannot hold securities*");
    }

    [Fact]
    public void RemoveShares_KeepsAverageCostAndRemovesEmptyPositions()
    {
        var account = CreateAccount(AccountType.Retirement);
        account.AddShares("VOO", 10m, 400m, Now);

        account.RemoveShares("voo", 4m, Now);
        account.Holdings.Single().Quantity.Should().Be(6m);
        account.Holdings.Single().AverageCostPerShare.Should().Be(400m);

        account.RemoveShares("VOO", 6m, Now);
        account.Holdings.Should().BeEmpty();
    }

    [Fact]
    public void RemoveShares_MoreThanHeld_Throws()
    {
        var account = CreateAccount(AccountType.Brokerage);
        account.AddShares("MSFT", 5m, 300m, Now);

        var act = () => account.RemoveShares("MSFT", 6m, Now);

        act.Should().Throw<DomainException>().WithMessage("*only 5 are held*");
    }

    [Fact]
    public void ArchivedAccount_RejectsChangesUntilRestored()
    {
        var account = CreateAccount(AccountType.Checking);
        account.Archive(Now);

        var act = () => account.RecordTransaction(Today, TransactionType.Deposit, 1m, "Test", Now);
        act.Should().Throw<DomainException>();

        account.Restore(Now);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(AccountType.CreditCard, true, false)]
    [InlineData(AccountType.Loan, true, false)]
    [InlineData(AccountType.Brokerage, false, true)]
    [InlineData(AccountType.Retirement, false, true)]
    [InlineData(AccountType.Checking, false, false)]
    public void AccountType_Classification(AccountType type, bool isLiability, bool supportsHoldings)
    {
        type.IsLiability().Should().Be(isLiability);
        type.SupportsHoldings().Should().Be(supportsHoldings);
    }

    private static Account CreateAccount(AccountType type, decimal openingBalance = 0m) =>
        new("Test account", type, institution: null, notes: null, openingBalance, Now);
}
