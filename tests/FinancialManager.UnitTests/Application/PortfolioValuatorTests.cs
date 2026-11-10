using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Portfolio;
using FinancialManager.Application.Quotes;
using FinancialManager.Domain.Accounts;
using FinancialManager.UnitTests.TestDoubles;

namespace FinancialManager.UnitTests.Application;

public sealed class PortfolioValuatorTests
{
    private static readonly DateTime AsOf = FakeClock.DefaultUtcNow;

    [Fact]
    public void ValuePortfolio_ComputesNetWorthFromAssetsLiabilitiesAndLivePrices()
    {
        var checking = CreateAccount(AccountType.Checking, cash: 1_000m);
        var card = CreateAccount(AccountType.CreditCard, cash: 300m);
        var brokerage = CreateAccount(
            AccountType.Brokerage,
            cash: 500m,
            new HoldingModel(Guid.NewGuid(), "AAPL", 10m, 100m, 1_000m));
        var quotes = new Dictionary<string, StockQuote> { ["AAPL"] = TestQuotes.Create("AAPL", 150m, previousClose: 140m) };

        var snapshot = PortfolioValuator.ValuePortfolio([checking, card, brokerage], quotes, AsOf);

        snapshot.TotalAssets.Should().Be(1_000m + 500m + 1_500m);
        snapshot.TotalLiabilities.Should().Be(300m);
        snapshot.NetWorth.Should().Be(2_700m);
        snapshot.InvestmentsValue.Should().Be(1_500m);
        snapshot.DayChange.Should().Be(100m);
        snapshot.UnpricedSymbols.Should().BeEmpty();
    }

    [Fact]
    public void ValueAccount_FallsBackToCostBasisWhenPriceUnavailable()
    {
        var account = CreateAccount(
            AccountType.Retirement,
            cash: 0m,
            new HoldingModel(Guid.NewGuid(), "XYZ", 4m, 25m, 100m));

        var valuation = PortfolioValuator.ValueAccount(account, new Dictionary<string, StockQuote>());

        var holding = valuation.Holdings.Single();
        holding.IsPriced.Should().BeFalse();
        holding.MarketValue.Should().BeNull();
        holding.EffectiveValue.Should().Be(100m);
        holding.UnrealizedGain.Should().BeNull();
        valuation.HasUnpricedHoldings.Should().BeTrue();
        valuation.DayChange.Should().BeNull();
    }

    [Fact]
    public void HoldingValuation_ComputesGainPercentage()
    {
        var valuation = new HoldingValuation(
            new HoldingModel(Guid.NewGuid(), "MSFT", 2m, 200m, 400m),
            TestQuotes.Create("MSFT", 250m));

        valuation.MarketValue.Should().Be(500m);
        valuation.UnrealizedGain.Should().Be(100m);
        valuation.UnrealizedGainPercent.Should().Be(0.25m);
    }

    private static AccountSummary CreateAccount(AccountType type, decimal cash, params HoldingModel[] holdings) => new(
        Guid.NewGuid(),
        type.ToString(),
        type,
        Institution: null,
        cash,
        type.IsLiability(),
        type.SupportsHoldings(),
        IsArchived: false,
        holdings,
        AsOf);
}
