namespace FinancialManager.Application.Common;

/// <summary>
/// Numeric bounds used by <see cref="System.ComponentModel.DataAnnotations.RangeAttribute"/> on request
/// models. Expressed as invariant-culture strings because attribute arguments cannot be decimals.
/// </summary>
public static class ValidationRanges
{
    public const string Zero = "0";
    public const string MinimumPositiveAmount = "0.01";
    public const string MinimumPositiveQuantity = "0.000001";
    public const string MaximumAmount = "999999999999";
}
