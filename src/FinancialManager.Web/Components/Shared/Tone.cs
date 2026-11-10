namespace FinancialManager.Web.Components.Shared;

/// <summary>
/// Maps a signed value to the CSS class used to color gains and losses.
/// </summary>
internal static class Tone
{
    public const string Positive = "tone-positive";
    public const string Negative = "tone-negative";

    public static string For(decimal? value, bool colorize) => (colorize, value) switch
    {
        (false, _) or (_, null) => string.Empty,
        (_, > 0) => Positive,
        (_, < 0) => Negative,
        _ => string.Empty,
    };
}
