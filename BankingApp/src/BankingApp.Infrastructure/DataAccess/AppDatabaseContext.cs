// <copyright file="AppDatabaseContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AppDatabaseContext class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
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
            entity.HasOne<User>().WithMany().HasForeignKey(c => c.UserId);
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
            entity.HasOne<Card>().WithMany().HasForeignKey(t => t.CardId);
            entity.HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId);
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
            entity.Property(n => n.Category).IsRequired().HasConversion<string>().HasMaxLength(30);
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
            entity.HasOne<Transaction>().WithMany().HasForeignKey(t => t.TransactionId);
            entity.HasOne<User>().WithMany().HasForeignKey(t => t.UserId);
            entity.HasOne<Category>().WithMany().HasForeignKey(t => t.CategoryId);
        });
    }
}