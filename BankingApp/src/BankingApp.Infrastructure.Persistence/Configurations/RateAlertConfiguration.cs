namespace BankingApp.Infrastructure.Persistence.Configurations;

using Domain.Aggregates.RateAlertAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RateAlertConfiguration : IEntityTypeConfiguration<RateAlert>
{
    public void Configure(EntityTypeBuilder<RateAlert> builder)
    {
        builder.ToTable("RateAlerts");
        builder.HasKey(rateAlert => rateAlert.Id);
        builder.Ignore(rateAlert => rateAlert.DomainEvents);

        builder.Property(rateAlert => rateAlert.UserId).IsRequired();
        builder.Property(rateAlert => rateAlert.BaseCurrency)
            .HasConversion(PersistenceValueConverters.CurrencyConverter)
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(rateAlert => rateAlert.QuoteCurrency)
            .HasConversion(PersistenceValueConverters.CurrencyConverter)
            .HasMaxLength(3)
            .IsRequired();
        builder.Property(rateAlert => rateAlert.TargetRate).HasColumnType("decimal(18,6)").IsRequired();
        builder.Property(rateAlert => rateAlert.IsTriggered).HasDefaultValue(false);
        builder.Property(rateAlert => rateAlert.IsBuyAlert).IsRequired();
        builder.Property(rateAlert => rateAlert.CreatedAt).IsRequired();
    }
}
