namespace FinancialManager.Application.Quotes;

/// <summary>
/// Raised when a market-data provider fails (network error, unexpected response, throttling).
/// </summary>
public sealed class QuoteProviderException : Exception
{
    public QuoteProviderException()
    {
    }

    public QuoteProviderException(string message)
        : base(message)
    {
    }

    public QuoteProviderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
