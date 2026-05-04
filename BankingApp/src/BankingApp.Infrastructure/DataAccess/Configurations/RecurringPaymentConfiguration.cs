// <copyright file="RecurringPaymentConfiguration.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
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

        entity.HasKey(r => r.Id);

        entity.Property(r => r.UserId).IsRequired();
        entity.Property(r => r.BillerId).IsRequired();
        entity.Property(r => r.SourceAccountId).IsRequired();

        entity.Property(r => r.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_RecurringPayment_Amount",
            "Amount > 0"));

        entity.Property(r => r.IsPayInFull)
            .IsRequired()
            .HasDefaultValue(false);

        entity.Property(r => r.Frequency)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_RecurringPayment_Frequency",
            "Frequency IN ('Daily', 'Weekly', 'BiWeekly', 'Monthly', 'Quarterly', 'Yearly')"));

        entity.Property(r => r.StartDate).IsRequired();
        entity.Property(r => r.EndDate).IsRequired(false);
        entity.Property(r => r.NextExecutionDate).IsRequired();

        entity.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(RecurringPaymentStatus.Active);

        entity.ToTable(t => t.HasCheckConstraint(
            "CK_RecurringPayment_Status",
            "Status IN ('Active', 'Paused', 'Cancelled')"));

        entity.Property(r => r.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        entity.HasOne<User>().WithMany().HasForeignKey(r => r.UserId);
    }
}
