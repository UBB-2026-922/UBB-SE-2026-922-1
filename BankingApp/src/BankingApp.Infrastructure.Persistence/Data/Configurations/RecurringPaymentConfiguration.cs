namespace BankingApp.Infrastructure.Persistence.Data.Configurations;

using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public sealed class RecurringPaymentConfiguration : IEntityTypeConfiguration<RecurringPayment>
{
    public void Configure(EntityTypeBuilder<RecurringPayment> builder)
    {
        builder.ToTable("RecurringPayments");
        builder.HasKey(recurringPayment => recurringPayment.Id);
        builder.Ignore(recurringPayment => recurringPayment.DomainEvents);

        builder.Property(recurringPayment => recurringPayment.UserId).IsRequired();
        builder.Property(recurringPayment => recurringPayment.BillerId).IsRequired();
        builder.Property(recurringPayment => recurringPayment.SourceAccountId).IsRequired();
        builder.Property(recurringPayment => recurringPayment.Amount).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(recurringPayment => recurringPayment.IsPayInFull).HasDefaultValue(false);
        builder.Property(recurringPayment => recurringPayment.Frequency).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(recurringPayment => recurringPayment.StartDate).IsRequired();
        builder.Property(recurringPayment => recurringPayment.EndDate);
        builder.Property(recurringPayment => recurringPayment.NextExecutionDate).IsRequired();
        builder.Property(recurringPayment => recurringPayment.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .HasDefaultValue(RecurringPaymentStatus.Active)
            .IsRequired();
        builder.Property(recurringPayment => recurringPayment.CreatedAt).IsRequired();
    }
}
