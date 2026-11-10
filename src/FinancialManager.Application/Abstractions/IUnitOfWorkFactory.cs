namespace FinancialManager.Application.Abstractions;

/// <summary>
/// Creates independent <see cref="IUnitOfWork"/> instances. Long-lived UI sessions (such as
/// Blazor Server circuits) must not share a single data context, so each operation gets its own.
/// </summary>
public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
