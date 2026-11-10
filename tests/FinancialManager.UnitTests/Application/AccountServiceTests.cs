using FinancialManager.Application.Accounts;
using FinancialManager.Application.Accounts.Requests;
using FinancialManager.Application.Common;
using FinancialManager.Domain.Accounts;
using FinancialManager.UnitTests.TestDoubles;
using Microsoft.Extensions.Logging.Abstractions;

namespace FinancialManager.UnitTests.Application;

/// <summary>
/// Exercises the account use cases against a real SQLite schema so EF Core mappings are covered too.
/// </summary>
public sealed class AccountServiceTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly FakeClock _clock = new();
    private readonly AccountService _service;

    public AccountServiceTests()
    {
        _service = new AccountService(_database.UnitOfWorkFactory, _clock, NullLogger<AccountService>.Instance);
    }

    public void Dispose() => _database.Dispose();

    [Fact]
    public async Task CreateAccount_PersistsAccount()
    {
        var id = await CreateAccountAsync(AccountType.Savings, openingBalance: 2_500.50m);

        var result = await _service.GetAccountAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Summary.Name.Should().Be("Test account");
        result.Value.Summary.CashBalance.Should().Be(2_500.50m);
        result.Value.CreatedUtc.Should().Be(FakeClock.DefaultUtcNow);
        result.Value.CreatedUtc.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public async Task CreateAccount_WithInvalidRequest_ReturnsValidationError()
    {
        var result = await _service.CreateAccountAsync(new CreateAccountRequest { Name = "" }, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task GetAccount_WhenMissing_ReturnsNotFound()
    {
        var result = await _service.GetAccountAsync(Guid.NewGuid(), CancellationToken.None);

        result.Error!.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task RecordTransaction_UpdatesBalanceAndPersistsTransaction()
    {
        var id = await CreateAccountAsync(AccountType.Checking, openingBalance: 100m);

        var result = await _service.RecordTransactionAsync(
            id,
            new RecordTransactionRequest
            {
                Date = new DateOnly(2025, 1, 10),
                Type = TransactionType.Deposit,
                Amount = 50.25m,
                Description = "Refund",
            },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var details = (await _service.GetAccountAsync(id, CancellationToken.None)).Value;
        details.Summary.CashBalance.Should().Be(150.25m);
        details.Transactions.Should().ContainSingle(t => t.Description == "Refund" && t.Amount == 50.25m);
    }

    [Fact]
    public async Task RemoveTransaction_RestoresBalance()
    {
        var id = await CreateAccountAsync(AccountType.Checking, openingBalance: 100m);
        await _service.RecordTransactionAsync(
            id,
            new RecordTransactionRequest { Date = _clock.Today, Type = TransactionType.Withdrawal, Amount = 30m, Description = "Lunch" },
            CancellationToken.None);
        var transactionId = (await _service.GetAccountAsync(id, CancellationToken.None)).Value.Transactions.Single().Id;

        var result = await _service.RemoveTransactionAsync(id, transactionId, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _service.GetAccountAsync(id, CancellationToken.None)).Value.Summary.CashBalance.Should().Be(100m);
    }

    [Fact]
    public async Task AddAndRemoveShares_PersistHoldings()
    {
        var id = await CreateAccountAsync(AccountType.Brokerage);

        (await _service.AddSharesAsync(id, new AddSharesRequest { Symbol = "msft", Quantity = 2.5m, PricePerShare = 400m }, CancellationToken.None))
            .IsSuccess.Should().BeTrue();
        (await _service.AddSharesAsync(id, new AddSharesRequest { Symbol = "MSFT", Quantity = 2.5m, PricePerShare = 420m }, CancellationToken.None))
            .IsSuccess.Should().BeTrue();
        (await _service.RemoveSharesAsync(id, new RemoveSharesRequest { Symbol = "MSFT", Quantity = 1m }, CancellationToken.None))
            .IsSuccess.Should().BeTrue();

        var holding = (await _service.GetAccountAsync(id, CancellationToken.None)).Value.Summary.Holdings.Single();
        holding.Symbol.Should().Be("MSFT");
        holding.Quantity.Should().Be(4m);
        holding.AverageCostPerShare.Should().Be(410m);
    }

    [Fact]
    public async Task AddShares_OnCheckingAccount_ReturnsValidationError()
    {
        var id = await CreateAccountAsync(AccountType.Checking);

        var result = await _service.AddSharesAsync(
            id,
            new AddSharesRequest { Symbol = "AAPL", Quantity = 1m, PricePerShare = 1m },
            CancellationToken.None);

        result.Error!.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task ArchivedAccounts_AreHiddenUnlessRequested()
    {
        var activeId = await CreateAccountAsync(AccountType.Checking);
        var archivedId = await CreateAccountAsync(AccountType.Savings);
        await _service.ArchiveAccountAsync(archivedId, CancellationToken.None);

        var active = await _service.GetAccountsAsync(includeArchived: false, CancellationToken.None);
        var all = await _service.GetAccountsAsync(includeArchived: true, CancellationToken.None);

        active.Select(a => a.Id).Should().BeEquivalentTo([activeId]);
        all.Select(a => a.Id).Should().BeEquivalentTo([activeId, archivedId]);
    }

    [Fact]
    public async Task DeleteAccount_RemovesAccountAndChildren()
    {
        var id = await CreateAccountAsync(AccountType.Brokerage);
        await _service.AddSharesAsync(id, new AddSharesRequest { Symbol = "VOO", Quantity = 1m, PricePerShare = 500m }, CancellationToken.None);

        var result = await _service.DeleteAccountAsync(id, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await _service.GetAccountAsync(id, CancellationToken.None)).Error!.Type.Should().Be(ErrorType.NotFound);
    }

    private async Task<Guid> CreateAccountAsync(AccountType type, decimal openingBalance = 0m)
    {
        var result = await _service.CreateAccountAsync(
            new CreateAccountRequest { Name = "Test account", Type = type, OpeningBalance = openingBalance },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        return result.Value;
    }
}
