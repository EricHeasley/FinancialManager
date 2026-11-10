namespace FinancialManager.Domain.Common;

/// <summary>
/// Field length and precision limits shared by the domain, validation and persistence layers.
/// </summary>
public static class DomainLimits
{
    public const int AccountNameMaxLength = 100;
    public const int InstitutionMaxLength = 100;
    public const int NotesMaxLength = 500;
    public const int TransactionDescriptionMaxLength = 200;
    public const int SymbolMaxLength = 15;

    /// <summary>Total number of digits stored for monetary values.</summary>
    public const int MoneyPrecision = 18;

    /// <summary>Number of decimal places stored for monetary values.</summary>
    public const int MoneyScale = 2;

    /// <summary>Total number of digits stored for share quantities and per-share prices.</summary>
    public const int QuantityPrecision = 18;

    /// <summary>Number of decimal places stored for share quantities (supports fractional shares).</summary>
    public const int QuantityScale = 6;

    /// <summary>Number of decimal places stored for per-share prices.</summary>
    public const int UnitPriceScale = 4;
}
