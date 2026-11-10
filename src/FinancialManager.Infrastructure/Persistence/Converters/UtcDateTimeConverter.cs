using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FinancialManager.Infrastructure.Persistence.Converters;

/// <summary>
/// SQLite does not store <see cref="DateTimeKind"/>. All timestamps in the model are UTC,
/// so values read back are marked as UTC explicitly.
/// </summary>
internal sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
