using System.ComponentModel.DataAnnotations;

namespace FinancialManager.Infrastructure.Persistence;

/// <summary>
/// Persistence settings bound from the <c>Database</c> configuration section.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Name of the entry in <c>ConnectionStrings</c> used for the application database.</summary>
    [Required]
    public string ConnectionStringName { get; set; } = string.Empty;

    /// <summary>Applies pending EF Core migrations when the application starts.</summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
