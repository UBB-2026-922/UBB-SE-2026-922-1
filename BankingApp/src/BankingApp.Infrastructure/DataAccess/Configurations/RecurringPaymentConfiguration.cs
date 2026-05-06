// <copyright file="RecurringPaymentConfiguration.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the RecurringPaymentConfiguration class.
// </summary>

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
    public void Configure(EntityTypeBuilder<RecurringPayment> entity)
    {
        entity.ToTable("RecurringPayment");

        entity.HasKey(recurringPayment => recurringPayment.Id);

        entity.Property(recurringPayment => recurringPayment.UserId).IsRequired();
        entity.Property(recurringPayment => recurringPayment.BillerId).IsRequired();
        entity.Property(recurringPayment => recurringPayment.SourceAccountId).IsRequired();

        entity.Property(recurringPayment => recurringPayment.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.ToTable(tableBuilder => tableBuilder.HasCheckConstraint(
            "CK_RecurringPayment_Amount",
            "Amount > 0"));

        entity.Property(recurringPayment => recurringPayment.IsPayInFull)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(recurringPayment => recurringPayment.Frequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.ToTable(tableBuilder => tableBuilder.HasCheckConstraint(
            "CK_RecurringPayment_Frequency",
            "Frequency IN ('Daily', 'Weekly', 'BiWeekly', 'Monthly', 'Quarterly', 'Yearly')"));

        entity.Property(recurringPayment => recurringPayment.StartDate).IsRequired();
        entity.Property(recurringPayment => recurringPayment.EndDate).IsRequired(false);
        entity.Property(recurringPayment => recurringPayment.NextExecutionDate).IsRequired();

        entity.Property(recurringPayment => recurringPayment.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(RecurringPaymentStatus.Active);

        entity.ToTable(tableBuilder => tableBuilder.HasCheckConstraint(
            "CK_RecurringPayment_Status",
            "Status IN ('Active', 'Paused', 'Cancelled')"));

        entity.Property(recurringPayment => recurringPayment.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        entity.HasIndex(recurringPayment => recurringPayment.BillerId);
        entity.HasIndex(recurringPayment => recurringPayment.SourceAccountId);
        entity.HasIndex(recurringPayment => recurringPayment.UserId);

        entity.HasOne<Biller>()
            .WithMany()
            .HasForeignKey(recurringPayment => recurringPayment.BillerId);

        entity.HasOne<Account>()
            .WithMany()
            .HasForeignKey(recurringPayment => recurringPayment.SourceAccountId)
            .OnDelete(DeleteBehavior.NoAction);

        entity.HasOne<User>().WithMany().HasForeignKey(recurringPayment => recurringPayment.UserId);
    }
}
