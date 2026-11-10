using FinancialManager.Application.Abstractions;
using FinancialManager.Application.Common;
using FinancialManager.Domain.Accounts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialManager.Application.Quotes;

public sealed partial class QuoteService : IQuoteService
{
    private readonly IStockQuoteProvider _provider;
    private readonly QuoteOptions _options;
    private readonly ILogger<QuoteService> _logger;

    public QuoteService(IStockQuoteProvider provider, IOptions<QuoteOptions> options, ILogger<QuoteService> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _provider = provider;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Result<StockQuote>> GetQuoteAsync(QuoteLookupRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationError = RequestValidator.Validate(request);
        if (validationError is not null)
        {
            return Result<StockQuote>.Failure(validationError);
        }

        var symbol = Holding.NormalizeSymbol(request.Symbol);
        try
        {
            var quote = await _provider.GetQuoteAsync(symbol, cancellationToken);
            return quote is null
                ? Result<StockQuote>.Failure(Error.NotFound($"No quote was found for '{symbol}'."))
                : Result<StockQuote>.Success(quote);
        }
        catch (QuoteProviderException ex)
        {
            LogQuoteFailure(_logger, symbol, ex);
            return Result<StockQuote>.Failure(
                Error.ExternalService("The market data service is currently unavailable. Please try again shortly."));
        }
    }

    public async Task<IReadOnlyDictionary<string, StockQuote>> GetQuotesAsync(
        IEnumerable<string> symbols,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(symbols);

        var distinctSymbols = symbols
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(Holding.NormalizeSymbol)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var quotes = new Dictionary<string, StockQuote>(StringComparer.Ordinal);
        if (distinctSymbols.Count == 0)
        {
            return quotes;
        }

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxConcurrentRequests,
            CancellationToken = cancellationToken,
        };

        var gate = new object();
        await Parallel.ForEachAsync(distinctSymbols, parallelOptions, async (symbol, token) =>
        {
            try
            {
                var quote = await _provider.GetQuoteAsync(symbol, token);
                if (quote is not null)
                {
                    lock (gate)
                    {
                        quotes[symbol] = quote;
                    }
                }
            }
            catch (QuoteProviderException ex)
            {
                LogQuoteFailure(_logger, symbol, ex);
            }
        });

        return quotes;
    }

    [LoggerMessage(EventId = 2001, Level = LogLevel.Warning, Message = "Failed to retrieve a quote for {Symbol}.")]
    private static partial void LogQuoteFailure(ILogger logger, string symbol, Exception exception);
}
