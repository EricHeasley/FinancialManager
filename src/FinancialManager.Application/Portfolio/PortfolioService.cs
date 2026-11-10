using FinancialManager.Application.Abstractions;
using FinancialManager.Application.Accounts;
using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Common;
using FinancialManager.Application.Quotes;

namespace FinancialManager.Application.Portfolio;

public sealed class PortfolioService : IPortfolioService
{
    private readonly IAccountService _accountService;
    private readonly IQuoteService _quoteService;
    private readonly IClock _clock;

    public PortfolioService(IAccountService accountService, IQuoteService quoteService, IClock clock)
    {
        _accountService = accountService;
        _quoteService = quoteService;
        _clock = clock;
    }

    public async Task<PortfolioSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var accounts = await _accountService.GetAccountsAsync(includeArchived: false, cancellationToken);
        var symbols = accounts.SelectMany(a => a.Holdings).Select(h => h.Symbol);
        var quotes = await _quoteService.GetQuotesAsync(symbols, cancellationToken);

        return PortfolioValuator.ValuePortfolio(accounts, quotes, _clock.UtcNow);
    }

    public async Task<Result<AccountValuation>> GetAccountValuationAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var accountResult = await _accountService.GetAccountAsync(accountId, cancellationToken);
        if (!accountResult.IsSuccess)
        {
            return Result<AccountValuation>.Failure(accountResult.Error);
        }

        var valuation = await ValueAccountAsync(accountResult.Value.Summary, cancellationToken);
        return Result<AccountValuation>.Success(valuation);
    }

    public async Task<AccountValuation> ValueAccountAsync(AccountSummary account, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(account);

        var quotes = await _quoteService.GetQuotesAsync(account.Holdings.Select(h => h.Symbol), cancellationToken);
        return PortfolioValuator.ValueAccount(account, quotes);
    }
}
