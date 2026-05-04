// <copyright file="AppDatabaseContext.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the AppDatabaseContext class.
// </summary>

using BankingApp.Domain.Entities;
using BankingApp.Domain.Enums;
using BankingApp.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

    /// <summary>Gets or sets the beneficiaries table.</summary>
    public DbSet<Beneficiary> Beneficiaries { get; set; }

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

    /// <summary>Gets or sets the billers table.</summary>
    public DbSet<Biller> Billers { get; set; }

    /// <summary>Gets or sets the saved billers table.</summary>
    public DbSet<SavedBiller> SavedBillers { get; set; }

    /// <summary>Gets or sets the transfers table.</summary>
    public DbSet<Transfer> Transfers { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("Beneficiary");
            entity.HasKey(beneficiary => beneficiary.Id);
            entity.Property(beneficiary => beneficiary.Name).IsRequired().HasMaxLength(200);
            entity.Property(beneficiary => beneficiary.Iban).IsRequired().HasMaxLength(34).HasColumnName("IBAN");
            entity.Property(beneficiary => beneficiary.BankName).HasMaxLength(200);
            entity.Property(beneficiary => beneficiary.TotalAmountSent).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(beneficiary => beneficiary.TransferCount).HasDefaultValue(0);
            entity.Property(beneficiary => beneficiary.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(beneficiary => new { beneficiary.UserId, beneficiary.Iban }).IsUnique();
            entity.HasOne<User>().WithMany().HasForeignKey(beneficiary => beneficiary.UserId);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(user => user.Id);
            entity.Property(user => user.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.PasswordHash).HasMaxLength(512);
            entity.Property(user => user.FullName).IsRequired().HasMaxLength(200);
            entity.Property(user => user.PhoneNumber).HasMaxLength(20);
            entity.Property(user => user.PreferredLanguage).HasMaxLength(5).HasDefaultValue("en");
            entity.Property(user => user.Is2FaEnabled).HasColumnName("Is2FAEnabled").HasDefaultValue(false);
            entity.Property(user => user.Preferred2FaMethod).HasConversion<string>();
            entity.Property(user => user.IsLocked).HasDefaultValue(false);
            entity.Property(user => user.FailedLoginAttempts).HasDefaultValue(0);
            entity.Property(user => user.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(user => user.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("Session");
            entity.HasKey(session => session.Id);
            entity.Property(session => session.Token).IsRequired().HasMaxLength(512);
            entity.Property(session => session.DeviceInfo).HasMaxLength(255);
            entity.Property(session => session.Browser).HasMaxLength(100);
            entity.Property(session => session.IpAddress).HasMaxLength(45);
            entity.Property(session => session.IsRevoked).HasDefaultValue(false);
            entity.Property(session => session.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(session => session.UserId);
        });

        modelBuilder.Entity<OAuthLink>(entity =>
        {
            entity.ToTable("OAuthLink");
            entity.HasKey(oAuthLink => oAuthLink.Id);
            entity.Property(oAuthLink => oAuthLink.Provider).IsRequired().HasMaxLength(20);
            entity.Property(oAuthLink => oAuthLink.ProviderUserId).IsRequired().HasMaxLength(255);
            entity.Property(oAuthLink => oAuthLink.ProviderEmail).HasMaxLength(255);
            entity.Property(oAuthLink => oAuthLink.LinkedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(oAuthLink => oAuthLink.UserId);
        });

        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");
            entity.HasKey(account => account.Id);
            entity.Property(account => account.AccountName).HasMaxLength(100);
            entity.Property(account => account.Iban).IsRequired().HasMaxLength(34).HasColumnName("IBAN");
            entity.HasIndex(account => account.Iban).IsUnique();
            entity.Property(account => account.Currency).IsRequired().HasMaxLength(3);
            entity.Property(account => account.Balance).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(account => account.AccountType).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(account => account.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue(AccountStatus.Active);
            entity.Property(account => account.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(account => account.UserId);
        });

        modelBuilder.Entity<Card>(entity =>
        {
            entity.ToTable("Card");
            entity.HasKey(card => card.Id);
            entity.Property(card => card.CardNumber).IsRequired().HasMaxLength(19);
            entity.Property(card => card.CardholderName).IsRequired().HasMaxLength(200);
            entity.Property(card => card.Cvv).IsRequired().HasMaxLength(4).HasColumnName("CVV");
            entity.Property(card => card.CardType).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(card => card.CardBrand).HasMaxLength(20);
            entity.Property(card => card.Status).IsRequired().HasConversion<string>().HasMaxLength(20).HasDefaultValue(CardStatus.Active);
            entity.Property(card => card.DailyTransactionLimit).HasColumnType("decimal(18,2)");
            entity.Property(card => card.MonthlySpendingCap).HasColumnType("decimal(18,2)");
            entity.Property(card => card.AtmWithdrawalLimit).HasColumnType("decimal(18,2)");
            entity.Property(card => card.ContactlessLimit).HasColumnType("decimal(18,2)");
            entity.Property(card => card.IsContactlessEnabled).HasDefaultValue(true);
            entity.Property(card => card.IsOnlineEnabled).HasDefaultValue(true);
            entity.Property(card => card.SortOrder).HasDefaultValue(0);
            entity.Property(card => card.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<Account>().WithMany().HasForeignKey(card => card.AccountId);
            entity.HasOne<User>().WithMany().HasForeignKey(card => card.UserId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(category => category.Id);
            entity.Property(category => category.Name).IsRequired().HasMaxLength(100);
            entity.Property(category => category.Icon).HasMaxLength(50);
            entity.Property(category => category.IsSystem).HasDefaultValue(true);
        });
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transaction");
            entity.HasKey(transaction => transaction.Id);
            entity.Property(transaction => transaction.TransactionRef).IsRequired().HasMaxLength(50);
            entity.HasIndex(transaction => transaction.TransactionRef).IsUnique();
            entity.Property(transaction => transaction.Direction).IsRequired().HasConversion<string>().HasMaxLength(10);
            entity.Property(transaction => transaction.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(transaction => transaction.Currency).IsRequired().HasMaxLength(3);
            entity.Property(transaction => transaction.BalanceAfter).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(transaction => transaction.CounterpartyName).HasMaxLength(200);
            entity.Property(transaction => transaction.CounterpartyIban).HasMaxLength(34).HasColumnName("CounterpartyIBAN");
            entity.Property(transaction => transaction.MerchantName).HasMaxLength(200);
            entity.Property(transaction => transaction.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(transaction => transaction.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(transaction => transaction.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(transaction => transaction.RelatedEntityType).HasMaxLength(50);
            entity.Property(transaction => transaction.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<Account>().WithMany().HasForeignKey(transaction => transaction.AccountId);
            entity.HasOne<Card>().WithMany().HasForeignKey(transaction => transaction.CardId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Category>().WithMany().HasForeignKey(transaction => transaction.CategoryId).OnDelete(DeleteBehavior.NoAction);
        });
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notification");
            entity.HasKey(notification => notification.Id);
            entity.Property(notification => notification.Title).IsRequired().HasMaxLength(200);
            entity.Property(notification => notification.Message).IsRequired();
            entity.Property(notification => notification.Type).IsRequired().HasMaxLength(30);
            entity.Property(notification => notification.Channel).IsRequired().HasMaxLength(20);
            entity.Property(notification => notification.IsRead).HasDefaultValue(false);
            entity.Property(notification => notification.RelatedEntityType).HasMaxLength(50);
            entity.Property(notification => notification.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(notification => notification.UserId);
        });

        var notificationTypeConverter = new ValueConverter<NotificationType, string>(
            type => type.ToDisplayName(),
            value => NotificationTypeExtensions.FromString(value));

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("NotificationPreference");
            entity.HasKey(notificationPreference => notificationPreference.Id);
            entity.Property(notificationPreference => notificationPreference.Category).IsRequired().HasConversion<string>().HasMaxLength(30);
            entity.Property(notificationPreference => notificationPreference.PushEnabled).HasDefaultValue(true);
            entity.Property(notificationPreference => notificationPreference.EmailEnabled).HasDefaultValue(true);
            entity.Property(notificationPreference => notificationPreference.SmsEnabled).HasDefaultValue(false);
            entity.Property(notificationPreference => notificationPreference.MinAmountThreshold).HasColumnType("decimal(18,2)");
            entity.HasOne<User>().WithMany().HasForeignKey(notificationPreference => notificationPreference.UserId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetToken");
            entity.HasKey(passwordResetToken => passwordResetToken.Id);
            entity.Property(passwordResetToken => passwordResetToken.TokenHash).IsRequired().HasMaxLength(512);
            entity.Property(passwordResetToken => passwordResetToken.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(passwordResetToken => passwordResetToken.UserId);
        });

        modelBuilder.Entity<TransactionCategoryOverride>(entity =>
        {
            entity.ToTable("TransactionCategoryOverride");
            entity.HasKey(transactionCategoryOverride => transactionCategoryOverride.Id);
            entity.HasOne<Transaction>().WithMany().HasForeignKey(transactionCategoryOverride => transactionCategoryOverride.TransactionId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<User>().WithMany().HasForeignKey(transactionCategoryOverride => transactionCategoryOverride.UserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Category>().WithMany().HasForeignKey(transactionCategoryOverride => transactionCategoryOverride.CategoryId).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Biller>(entity =>
        {
            entity.ToTable("Biller");
            entity.HasKey(biller => biller.Id);
            entity.Property(biller => biller.Name).IsRequired().HasMaxLength(200);
            entity.Property(biller => biller.Category).IsRequired().HasMaxLength(100);
            entity.Property(biller => biller.LogoUrl).HasMaxLength(500);
            entity.Property(biller => biller.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<SavedBiller>(entity =>
        {
            entity.ToTable("SavedBiller");
            entity.HasKey(savedBiller => savedBiller.Id);
            entity.Property(savedBiller => savedBiller.Nickname).HasMaxLength(100);
            entity.Property(savedBiller => savedBiller.DefaultReference).HasMaxLength(200);
            entity.Property(savedBiller => savedBiller.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(savedBiller => savedBiller.Biller).WithMany().HasForeignKey(savedBiller => savedBiller.BillerId);
            entity.HasOne<User>().WithMany().HasForeignKey(savedBiller => savedBiller.UserId);
        });

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.ToTable("Transfers");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.RecipientName).IsRequired().HasMaxLength(200);
            entity.Property(t => t.RecipientIban).IsRequired().HasMaxLength(50);
            entity.Property(t => t.RecipientBankName).HasMaxLength(200);
            entity.Property(t => t.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(t => t.Currency).IsRequired().HasMaxLength(10);
            entity.Property(t => t.ConvertedAmount).HasColumnType("decimal(18,2)");
            entity.Property(t => t.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(t => t.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(t => t.Reference).HasMaxLength(200);
            entity.Property(t => t.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne<User>().WithMany().HasForeignKey(t => t.UserId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Account>().WithMany().HasForeignKey(t => t.SourceAccountId).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne<Transaction>().WithMany().HasForeignKey(t => t.TransactionId).OnDelete(DeleteBehavior.NoAction);
        });
    }
}
