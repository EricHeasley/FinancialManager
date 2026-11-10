using FinancialManager.Application.Abstractions;
using FinancialManager.Application.Accounts.Models;
using FinancialManager.Application.Accounts.Requests;
using FinancialManager.Application.Common;
using FinancialManager.Domain.Accounts;
using FinancialManager.Domain.Common;
using Microsoft.Extensions.Logging;

namespace FinancialManager.Application.Accounts;

public sealed partial class AccountService : IAccountService
{
    private const string AccountNotFoundMessage = "The account could not be found.";

    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IClock _clock;
    private readonly ILogger<AccountService> _logger;

    public AccountService(IUnitOfWorkFactory unitOfWorkFactory, IClock clock, ILogger<AccountService> logger)
    {
        _unitOfWorkFactory = unitOfWorkFactory;
        _clock = clock;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AccountSummary>> GetAccountsAsync(bool includeArchived, CancellationToken cancellationToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        var accounts = await unitOfWork.Accounts.ListAsync(includeArchived, cancellationToken);
        return accounts
            .OrderBy(a => a.IsArchived)
            .ThenBy(a => a.IsLiability)
            .ThenBy(a => a.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(AccountMapper.ToSummary)
            .ToList();
    }

    public async Task<Result<AccountDetails>> GetAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        var account = await unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
        return account is null
            ? Result<AccountDetails>.Failure(Error.NotFound(AccountNotFoundMessage))
            : Result<AccountDetails>.Success(AccountMapper.ToDetails(account));
    }

    public async Task<Result<Guid>> CreateAccountAsync(CreateAccountRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationError = RequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result<Guid>.Failure(validationError);
        }

        Account account;
        try
        {
            account = new Account(
                request.Name,
                request.Type,
                request.Institution,
                request.Notes,
                request.OpeningBalance,
                _clock.UtcNow);
        }
        catch (DomainException ex)
        {
            return Result<Guid>.Failure(Error.Validation(ex.Message));
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        unitOfWork.Accounts.Add(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogAccountCreated(_logger, account.Id, account.Type);
        return Result<Guid>.Success(account.Id);
    }

    public Task<Result> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            accountId,
            request,
            (account, utcNow) => account.UpdateDetails(request.Name, request.Institution, request.Notes, utcNow),
            cancellationToken);
    }

    public Task<Result> ArchiveAccountAsync(Guid accountId, CancellationToken cancellationToken) =>
        ExecuteAsync(accountId, request: null, (account, utcNow) => account.Archive(utcNow), cancellationToken);

    public Task<Result> RestoreAccountAsync(Guid accountId, CancellationToken cancellationToken) =>
        ExecuteAsync(accountId, request: null, (account, utcNow) => account.Restore(utcNow), cancellationToken);

    public async Task<Result> DeleteAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        await using var unitOfWork = _unitOfWorkFactory.Create();
        var account = await unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
        {
            return Result.Failure(Error.NotFound(AccountNotFoundMessage));
        }

        unitOfWork.Accounts.Remove(account);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        LogAccountDeleted(_logger, accountId);
        return Result.Success();
    }

    public Task<Result> RecordTransactionAsync(Guid accountId, RecordTransactionRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            accountId,
            request,
            (account, utcNow) => account.RecordTransaction(
                request.Date ?? _clock.Today,
                request.Type,
                request.Amount,
                request.Description,
                utcNow),
            cancellationToken);
    }

    public Task<Result> RemoveTransactionAsync(Guid accountId, Guid transactionId, CancellationToken cancellationToken) =>
        ExecuteAsync(
            accountId,
            request: null,
            (account, utcNow) => account.RemoveTransaction(transactionId, utcNow),
            cancellationToken);

    public Task<Result> AddSharesAsync(Guid accountId, AddSharesRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            accountId,
            request,
            (account, utcNow) => account.AddShares(request.Symbol, request.Quantity, request.PricePerShare, utcNow),
            cancellationToken);
    }

    public Task<Result> RemoveSharesAsync(Guid accountId, RemoveSharesRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        return ExecuteAsync(
            accountId,
            request,
            (account, utcNow) => account.RemoveShares(request.Symbol, request.Quantity, utcNow),
            cancellationToken);
    }

    /// <summary>
    /// Shared load → validate → mutate → save pipeline for commands that modify an existing account.
    /// </summary>
    private async Task<Result> ExecuteAsync(
        Guid accountId,
        object? request,
        Action<Account, DateTime> mutation,
        CancellationToken cancellationToken)
    {
        if (request is not null)
        {
            var validationError = RequestValidator.Validate(request);
            if (validationError is not null)
            {
                return Result.Failure(validationError);
            }
        }

        await using var unitOfWork = _unitOfWorkFactory.Create();
        var account = await unitOfWork.Accounts.GetByIdAsync(accountId, cancellationToken);
        if (account is null)
        {
            return Result.Failure(Error.NotFound(AccountNotFoundMessage));
        }

        try
        {
            mutation(account, _clock.UtcNow);
        }
        catch (DomainException ex)
        {
            return Result.Failure(Error.Validation(ex.Message));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    [LoggerMessage(EventId = 1001, Level = LogLevel.Information, Message = "Created account {AccountId} of type {AccountType}.")]
    private static partial void LogAccountCreated(ILogger logger, Guid accountId, AccountType accountType);

    [LoggerMessage(EventId = 1002, Level = LogLevel.Information, Message = "Deleted account {AccountId}.")]
    private static partial void LogAccountDeleted(ILogger logger, Guid accountId);
}
