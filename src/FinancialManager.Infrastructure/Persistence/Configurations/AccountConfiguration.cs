using FinancialManager.Domain.Accounts;
using FinancialManager.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialManager.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Name).HasMaxLength(DomainLimits.AccountNameMaxLength).IsRequired();
        builder.Property(a => a.Institution).HasMaxLength(DomainLimits.InstitutionMaxLength);
        builder.Property(a => a.Notes).HasMaxLength(DomainLimits.NotesMaxLength);
        builder.Property(a => a.Type).IsRequired();
        builder.Property(a => a.OpeningBalance).HasPrecision(DomainLimits.MoneyPrecision, DomainLimits.MoneyScale);
        builder.Property(a => a.CashBalance).HasPrecision(DomainLimits.MoneyPrecision, DomainLimits.MoneyScale);
        builder.Property(a => a.IsArchived).IsRequired();

        builder.Ignore(a => a.IsLiability);
        builder.Ignore(a => a.SupportsHoldings);

        builder.HasMany(a => a.Transactions)
            .WithOne()
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(a => a.Holdings)
            .WithOne()
            .HasForeignKey(h => h.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(a => a.Transactions).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(a => a.Holdings).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(a => a.IsArchived);
    }
}
