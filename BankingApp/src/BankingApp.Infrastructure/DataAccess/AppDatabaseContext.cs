// <copyright file="AppDatabaseContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AppDatabaseContext class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infrastructure.DataAccess;

/// <summary>
///     Provides the EF Core database context for the BankingApp.
/// </summary>
public class AppDatabaseContext : DbContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="AppDatabaseContext" /> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public AppDatabaseContext(DbContextOptions<AppDatabaseContext> options) : base(options)
    {
    }

    /// <summary>Gets or sets the users table.</summary>
    public DbSet<User> Users { get; set; }

    /// <summary>Gets or sets the sessions table.</summary>
    public DbSet<Session> Sessions { get; set; }

    /// <summary>Gets or sets the OAuth links table.</summary>
    public DbSet<OAuthLink> OAuthLinks { get; set; }

    /// <summary>Gets or sets the accounts table.</summary>
    public DbSet<Account> Accounts { get; set; }

    /// <summary>Gets or sets the cards table.</summary>
    public DbSet<Card> Cards { get; set; }

    /// <summary>Gets or sets the categories table.</summary>
    public DbSet<Category> Categories { get; set; }

    /// <summary>Gets or sets the transactions table.</summary>
    public DbSet<Transaction> Transactions { get; set; }

    /// <summary>Gets or sets the notifications table.</summary>
    public DbSet<Notification> Notifications { get; set; }

    /// <summary>Gets or sets the notification preferences table.</summary>
    public DbSet<NotificationPreference> NotificationPreferences { get; set; }

    /// <summary>Gets or sets the password reset tokens table.</summary>
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    /// <summary>Gets or sets the transaction category overrides table.</summary>
    public DbSet<TransactionCategoryOverride> TransactionCategoryOverrides { get; set; }

    // ── Team B ─────────────────────────────────────────────────────────────

    /// <summary>Gets or sets the transfers table (Team B — bank transfer feature).</summary>
    public DbSet<Transfer> Transfers { get; set; }

    /// <summary>Gets or sets the beneficiaries table (Team B — address-book feature).</summary>
    public DbSet<Beneficiary> Beneficiaries { get; set; }

    /// <summary>Gets or sets the billers master table (Team B — bill payment feature).</summary>
    public DbSet<Biller> Billers { get; set; }

    /// <summary>Gets or sets the bill payments table (Team B — bill payment feature).</summary>
    public DbSet<BillPayment> BillPayments { get; set; }

    /// <summary>Gets or sets the saved billers table (Team B — quick-pay feature).</summary>
    public DbSet<SavedBiller> SavedBillers { get; set; }

    /// <summary>Gets or sets the recurring payments table (Team B — recurring payment feature).</summary>
    public DbSet<RecurringPayment> RecurringPayments { get; set; }

    /// <summary>Gets or sets the exchange transactions table (Team B — FX exchange feature).</summary>
    public DbSet<ExchangeTransaction> ExchangeTransactions { get; set; }

    /// <summary>Gets or sets the rate alerts table (Team B — FX rate alert feature).</summary>
    public DbSet<RateAlert> RateAlerts { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.PasswordHash).HasMaxLength(512);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            entity.Property(u => u.PhoneNumber).HasMaxLength(20);
            entity.Property(u => u.PreferredLanguage).HasMaxLength(5).HasDefaultValue("en");
            entity.Property(u => u.Is2FaEnabled).HasColumnName("Is2FAEnabled").HasDefaultValue(false);
            entity.Property(u => u.Preferred2FaMethod).HasConversion<string>();
            entity.Property(u => u.IsLocked).HasDefaultValue(false);
            entity.Property(u => u.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Session");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Token).IsRequired().HasMaxLength(512);
            entity.Property(s => s.DeviceInfo).HasMaxLength(255);
            entity.Property(s => s.Browser).HasMaxLength(100);
            entity.Property(s => s.IpAddress).HasMaxLength(45);
            entity.Property(s => s.IsRevoked).HasDefaultValue(false);
            entity.Property(s => s.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(s => s.UserId);
        });

        modelBuilder.Entity<OAuthLink>(entity =>
        {
            entity.ToTable("OAuthLink");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.Provider).IsRequired().HasMaxLength(20);
            entity.Property(o => o.ProviderUserId).IsRequired().HasMaxLength(255);
            entity.Property(o => o.ProviderEmail).HasMaxLength(255);
            entity.Property(o => o.LinkedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(o => o.UserId);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.AccountName).HasMaxLength(100);
            entity.Property(a => a.Iban).IsRequired().HasMaxLength(34).HasColumnName("IBAN");
            entity.HasIndex(a => a.Iban).IsUnique();
            entity.Property(a => a.Currency).IsRequired().HasMaxLength(3);
            entity.Property(a => a.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(a => a.AccountType).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(a => a.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(AccountStatus.Active);
            entity.Property(a => a.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(a => a.UserId);
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("Card");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.CardNumber).IsRequired().HasMaxLength(19);
            entity.Property(c => c.CardholderName).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Cvv).IsRequired().HasMaxLength(4).HasColumnName("CVV");
            entity.Property(c => c.CardType).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(c => c.CardBrand).HasMaxLength(20);
            entity.Property(c => c.Status).IsRequired().HasConversion<string>().HasMaxLength(20).HasDefaultValue(CardStatus.Active);
            entity.Property(c => c.DailyTransactionLimit).HasColumnType("decimal(18,2)");
            entity.Property(c => c.MonthlySpendingCap).HasColumnType("decimal(18,2)");
            entity.Property(c => c.AtmWithdrawalLimit).HasColumnType("decimal(18,2)");
            entity.Property(c => c.ContactlessLimit).HasColumnType("decimal(18,2)");
            entity.Property(c => c.IsContactlessEnabled).HasDefaultValue(true);
            entity.Property(c => c.IsOnlineEnabled).HasDefaultValue(true);
            entity.Property(c => c.SortOrder).HasDefaultValue(0);
            entity.Property(c => c.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<Account>().WithMany().HasForeignKey(c => c.AccountId);
            entity.HasOne<User>().WithMany().HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
            entity.Property(c => c.Icon).HasMaxLength(50);
            entity.Property(c => c.IsSystem).HasDefaultValue(true);
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transaction");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.TransactionRef).IsRequired().HasMaxLength(50);
            entity.HasIndex(t => t.TransactionRef).IsUnique();
            entity.Property(t => t.Type).IsRequired().HasMaxLength(30);
            entity.Property(t => t.Direction).IsRequired().HasConversion<string>().HasMaxLength(10);
            entity.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(t => t.Currency).IsRequired().HasMaxLength(3);
            entity.Property(t => t.BalanceAfter).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(t => t.CounterpartyName).HasMaxLength(200);
            entity.Property(t => t.CounterpartyIban).HasMaxLength(34).HasColumnName("CounterpartyIBAN");
            entity.Property(t => t.MerchantName).HasMaxLength(200);
            entity.Property(t => t.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(t => t.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.RelatedEntityType).HasMaxLength(50);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<Account>().WithMany().HasForeignKey(t => t.AccountId);
            entity.HasOne<Card>().WithMany().HasForeignKey(t => t.CardId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Title).IsRequired().HasMaxLength(200);
            entity.Property(n => n.Message).IsRequired();
            entity.Property(n => n.Type).IsRequired().HasMaxLength(30);
            entity.Property(n => n.Channel).IsRequired().HasMaxLength(20);
            entity.Property(n => n.IsRead).HasDefaultValue(false);
            entity.Property(n => n.RelatedEntityType).HasMaxLength(50);
            entity.Property(n => n.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(n => n.UserId);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("NotificationPreference");
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Category)
                .IsRequired()
                .HasConversion(
                    type => type.ToDisplayName(),
                    value => NotificationTypeExtensions.FromString(value))
                .HasMaxLength(30);
            entity.Property(n => n.PushEnabled).HasDefaultValue(true);
            entity.Property(n => n.EmailEnabled).HasDefaultValue(true);
            entity.Property(n => n.SmsEnabled).HasDefaultValue(false);
            entity.Property(n => n.MinAmountThreshold).HasColumnType("decimal(18,2)");
            entity.HasOne<User>().WithMany().HasForeignKey(n => n.UserId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetToken");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.TokenHash).IsRequired().HasMaxLength(512);
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(p => p.UserId);
        });

        modelBuilder.Entity<TransactionCategoryOverride>(entity =>
        {
            entity.ToTable("TransactionCategoryOverride");
            entity.HasKey(t => t.Id);
            entity.HasOne<Transaction>().WithMany().HasForeignKey(t => t.TransactionId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId).OnDelete(DeleteBehavior.NoAction);
        });

        // ── Team B EF mappings ────────────────────────────────────────────────

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.ToTable("Transfers");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.RecipientName).IsRequired().HasMaxLength(200);
            entity.Property(t => t.RecipientIban).IsRequired().HasMaxLength(50).HasColumnName("RecipientIBAN");
            entity.Property(t => t.RecipientBankName).HasMaxLength(200);
            entity.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(t => t.Currency).IsRequired().HasMaxLength(10);
            entity.Property(t => t.ConvertedAmount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(t => t.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(t => t.Reference).HasMaxLength(200);
            entity.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(t => t.UserId);
            entity.HasOne<Account>().WithMany().HasForeignKey(t => t.SourceAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Transaction>().WithMany().HasForeignKey(t => t.TransactionId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("Beneficiaries");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.Property(b => b.Iban).IsRequired().HasMaxLength(50).HasColumnName("IBAN");
            entity.HasIndex(b => new { b.UserId, b.Iban }).IsUnique();
            entity.Property(b => b.BankName).HasMaxLength(200);
            entity.Property(b => b.TotalAmountSent).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(b => b.TransferCount).HasDefaultValue(0);
            entity.Property(b => b.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(b => b.UserId);
        });

        modelBuilder.Entity<Biller>(entity =>
        {
            entity.ToTable("Biller");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(b => b.Name).IsUnique();
            entity.Property(b => b.Category).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(b => b.LogoUrl).HasMaxLength(500);
            entity.Property(b => b.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<BillPayment>(entity =>
        {
            entity.ToTable("BillPayment");
            entity.HasKey(bp => bp.Id);
            entity.Property(bp => bp.BillerReference).IsRequired().HasMaxLength(200);
            entity.Property(bp => bp.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(bp => bp.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(bp => bp.ReceiptNumber).IsRequired().HasMaxLength(100);
            entity.Property(bp => bp.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(bp => bp.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(bp => bp.UserId);
            entity.HasOne<Account>().WithMany().HasForeignKey(bp => bp.SourceAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Biller>().WithMany().HasForeignKey(bp => bp.BillerId);
            entity.HasOne<Transaction>().WithMany().HasForeignKey(bp => bp.TransactionId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SavedBiller>(entity =>
        {
            entity.ToTable("SavedBiller");
            entity.HasKey(sb => sb.Id);
            entity.Property(sb => sb.Nickname).HasMaxLength(200);
            entity.Property(sb => sb.DefaultReference).HasMaxLength(200);
            entity.Property(sb => sb.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(sb => sb.UserId);
            entity.HasOne<Biller>().WithMany().HasForeignKey(sb => sb.BillerId);
        });

        modelBuilder.Entity<RecurringPayment>(entity =>
        {
            entity.ToTable("RecurringPayment");
            entity.HasKey(rp => rp.Id);
            entity.Property(rp => rp.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(rp => rp.IsPayInFull).HasDefaultValue(false);
            entity.Property(rp => rp.Frequency).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(rp => rp.Status).IsRequired().HasConversion<string>().HasMaxLength(20).HasDefaultValue(RecurringPaymentStatus.Active);
            entity.Property(rp => rp.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(rp => rp.UserId);
            entity.HasOne<Account>().WithMany().HasForeignKey(rp => rp.SourceAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Biller>().WithMany().HasForeignKey(rp => rp.BillerId);
        });

        modelBuilder.Entity<ExchangeTransaction>(entity =>
        {
            entity.ToTable("ExchangeTransaction");
            entity.HasKey(et => et.Id);
            entity.Property(et => et.SourceCurrency).IsRequired().HasMaxLength(3);
            entity.Property(et => et.TargetCurrency).IsRequired().HasMaxLength(3);
            entity.Property(et => et.SourceAmount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(et => et.TargetAmount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(et => et.ExchangeRate).IsRequired().HasColumnType("decimal(18,6)");
            entity.Property(et => et.Commission).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(et => et.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(et => et.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(et => et.UserId);
            entity.HasOne<Account>().WithMany().HasForeignKey(et => et.SourceAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Account>().WithMany().HasForeignKey(et => et.TargetAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Transaction>().WithMany().HasForeignKey(et => et.TransactionId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RateAlert>(entity =>
        {
            entity.ToTable("RateAlert");
            entity.HasKey(ra => ra.Id);
            entity.Property(ra => ra.BaseCurrency).IsRequired().HasMaxLength(3);
            entity.Property(ra => ra.TargetCurrency).IsRequired().HasMaxLength(3);
            entity.Property(ra => ra.TargetRate).IsRequired().HasColumnType("decimal(18,6)");
            entity.Property(ra => ra.IsTriggered).HasDefaultValue(false);
            entity.Property(ra => ra.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(ra => ra.UserId);
        });
    }
}
