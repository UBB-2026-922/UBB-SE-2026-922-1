using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApp.Infrastructure.DataAccess.Configurations;

/// <summary>
///     Configures the EF Core mapping for the <see cref="RecurringPayment" /> entity
///     using Fluent API constraints derived from the legacy SQL schema.
/// </summary>
public class RecurringPaymentConfiguration : IEntityTypeConfiguration<RecurringPayment>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<RecurringPayment> builder)
    {
        builder.ToTable("RecurringPayment");

        builder.HasKey(recurringPayment => recurringPayment.Id);

        builder.Property(recurringPayment => recurringPayment.UserId).IsRequired();
        builder.Property(recurringPayment => recurringPayment.BillerId).IsRequired();
        builder.Property(recurringPayment => recurringPayment.SourceAccountId).IsRequired();

        builder.Property(recurringPayment => recurringPayment.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.ToTable(tableBuilder => tableBuilder.HasCheckConstraint(
            "CK_RecurringPayment_Amount",
            "Amount > 0"));

        builder.Property(recurringPayment => recurringPayment.IsPayInFull)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(recurringPayment => recurringPayment.Frequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.ToTable(tableBuilder => tableBuilder.HasCheckConstraint(
            "CK_RecurringPayment_Frequency",
            "Frequency IN ('Daily', 'Weekly', 'BiWeekly', 'Monthly', 'Quarterly', 'Yearly')"));

        builder.Property(recurringPayment => recurringPayment.StartDate).IsRequired();
        builder.Property(recurringPayment => recurringPayment.EndDate).IsRequired(false);
        builder.Property(recurringPayment => recurringPayment.NextExecutionDate).IsRequired();

        builder.Property(recurringPayment => recurringPayment.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(RecurringPaymentStatus.Active);

        builder.ToTable(tableBuilder => tableBuilder.HasCheckConstraint(
            "CK_RecurringPayment_Status",
            "Status IN ('Active', 'Paused', 'Cancelled')"));

        builder.Property(recurringPayment => recurringPayment.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(recurringPayment => recurringPayment.BillerId);
        builder.HasIndex(recurringPayment => recurringPayment.SourceAccountId);
        builder.HasIndex(recurringPayment => recurringPayment.UserId);

        builder.HasOne<Biller>()
            .WithMany()
            .HasForeignKey(recurringPayment => recurringPayment.BillerId);

        builder.HasOne<Account>()
            .WithMany()
            .HasForeignKey(recurringPayment => recurringPayment.SourceAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne<User>().WithMany().HasForeignKey(recurringPayment => recurringPayment.UserId);
    }
}
