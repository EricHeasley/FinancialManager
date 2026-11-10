namespace FinancialManager.Application.Common;

/// <summary>
/// Regular expressions shared by request models.
/// </summary>
public static class ValidationPatterns
{
    /// <summary>
    /// Ticker symbols such as <c>AAPL</c>, <c>BRK.B</c>, <c>^GSPC</c>, <c>EURUSD=X</c> or <c>BTC-USD</c>.
    /// </summary>
    public const string TickerSymbol = @"^\s*[A-Za-z0-9\.\-\^=]+\s*$";
}
