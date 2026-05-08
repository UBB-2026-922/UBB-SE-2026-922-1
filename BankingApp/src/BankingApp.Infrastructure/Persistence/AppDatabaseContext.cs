namespace BankingApp.Infrastructure.Persistence;

using Configurations;
using Domain.Entities;
using Domain.Enums;
using Domain.Aggregates.AccountAggregate;
using Domain.Aggregates.BeneficiaryAggregate;
using Domain.Aggregates.BillerAggregate;
using Domain.Aggregates.BillPaymentAggregate;
using Domain.Aggregates.ForexAggregate;
using Domain.Aggregates.RateAlertAggregate;
using Domain.Aggregates.RecurringPaymentAggregate;
using Domain.Aggregates.TransactionAggregate;
using Microsoft.EntityFrameworkCore;

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

    /// <summary>Gets or sets the billers table.</summary>
    public DbSet<Biller> Billers { get; set; }

    /// <summary>Gets or sets the bill payments table.</summary>
    public DbSet<BillPayment> BillPayments { get; set; }

    /// <summary>Gets or sets the saved billers table.</summary>
    public DbSet<SavedBiller> SavedBillers { get; set; }

    /// <summary>Gets or sets the password reset tokens table.</summary>
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

    /// <summary>Gets or sets the transaction category overrides table.</summary>
    public DbSet<TransactionCategoryOverride> TransactionCategoryOverrides { get; set; }

    /// <summary>Gets or sets the transfers table.</summary>
    public DbSet<Transfer> Transfers { get; set; }

    /// <summary>Gets or sets the recurring payments table.</summary>
    public DbSet<RecurringPayment> RecurringPayments { get; set; }

    /// <summary>Gets or sets the exchange transactions table.</summary>
    public DbSet<ForexTransaction> ExchangeTransactions { get; set; }

    /// <summary>Gets or sets the rate alerts table.</summary>
    public DbSet<RateAlert> RateAlerts { get; set; }

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
            entity.Property(beneficiary => beneficiary.TotalAmountSent).HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
            entity.Property(beneficiary => beneficiary.TransferCount).HasDefaultValue(0);
            entity.Property(beneficiary => beneficiary.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(beneficiary => new { beneficiary.UserId, beneficiary.Iban }).IsUnique();
            entity.HasOne(beneficiary => beneficiary.User).WithMany().HasForeignKey(beneficiary => beneficiary.UserId);
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
            entity.HasOne(session => session.User).WithMany().HasForeignKey(session => session.UserId);
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
            entity.Property(account => account.Status).HasConversion<string>().HasMaxLength(20)
                .HasDefaultValue(AccountStatus.Active);
            entity.Property(account => account.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(account => account.User).WithMany().HasForeignKey(account => account.UserId);
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
            entity.Property(card => card.Status).IsRequired().HasConversion<string>().HasMaxLength(20)
                .HasDefaultValue(CardStatus.Active);
            entity.Property(card => card.DailyTransactionLimit).HasColumnType("decimal(18,2)");
            entity.Property(card => card.MonthlySpendingCap).HasColumnType("decimal(18,2)");
            entity.Property(card => card.AtmWithdrawalLimit).HasColumnType("decimal(18,2)");
            entity.Property(card => card.ContactlessLimit).HasColumnType("decimal(18,2)");
            entity.Property(card => card.IsContactlessEnabled).HasDefaultValue(true);
            entity.Property(card => card.IsOnlineEnabled).HasDefaultValue(true);
            entity.Property(card => card.SortOrder).HasDefaultValue(0);
            entity.Property(card => card.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(card => card.Account).WithMany().HasForeignKey(card => card.AccountId);
            entity.HasOne(card => card.User).WithMany().HasForeignKey(card => card.UserId).OnDelete(DeleteBehavior.NoAction);
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
            entity.Property(transaction => transaction.CounterpartyIban).HasMaxLength(34)
                .HasColumnName("CounterpartyIBAN");
            entity.Property(transaction => transaction.MerchantName).HasMaxLength(200);
            entity.Property(transaction => transaction.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0);
            entity.Property(transaction => transaction.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(transaction => transaction.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
            entity.Property(transaction => transaction.RelatedEntityType).HasMaxLength(50);
            entity.Property(transaction => transaction.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(transaction => transaction.Account).WithMany().HasForeignKey(transaction => transaction.AccountId);
            entity.HasOne(transaction => transaction.Card).WithMany().HasForeignKey(transaction => transaction.CardId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(transaction => transaction.Category).WithMany().HasForeignKey(transaction => transaction.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
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
            entity.HasOne(notification => notification.User).WithMany().HasForeignKey(notification => notification.UserId);
        });

        modelBuilder.Entity<NotificationPreference>(entity =>
        {
            entity.ToTable("NotificationPreference");
            entity.HasKey(notificationPreference => notificationPreference.Id);
            entity.Property(notificationPreference => notificationPreference.Category).IsRequired()
                .HasConversion<string>().HasMaxLength(30);
            entity.Property(notificationPreference => notificationPreference.PushEnabled).HasDefaultValue(true);
            entity.Property(notificationPreference => notificationPreference.EmailEnabled).HasDefaultValue(true);
            entity.Property(notificationPreference => notificationPreference.SmsEnabled).HasDefaultValue(false);
            entity.Property(notificationPreference => notificationPreference.MinAmountThreshold)
                .HasColumnType("decimal(18,2)");
            entity.HasOne(notificationPreference => notificationPreference.User).WithMany().HasForeignKey(notificationPreference => notificationPreference.UserId);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetToken");
            entity.HasKey(passwordResetToken => passwordResetToken.Id);
            entity.Property(passwordResetToken => passwordResetToken.TokenHash).IsRequired().HasMaxLength(512);
            entity.Property(passwordResetToken => passwordResetToken.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(passwordResetToken => passwordResetToken.User).WithMany().HasForeignKey(passwordResetToken => passwordResetToken.UserId);
        });

        modelBuilder.Entity<TransactionCategoryOverride>(entity =>
        {
            entity.ToTable("TransactionCategoryOverride");
            entity.HasKey(transactionCategoryOverride => transactionCategoryOverride.Id);
            entity.HasOne(transactionCategoryOverride => transactionCategoryOverride.Transaction).WithMany()
                .HasForeignKey(transactionCategoryOverride => transactionCategoryOverride.TransactionId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(transactionCategoryOverride => transactionCategoryOverride.User).WithMany()
                .HasForeignKey(transactionCategoryOverride => transactionCategoryOverride.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(transactionCategoryOverride => transactionCategoryOverride.Category).WithMany()
                .HasForeignKey(transactionCategoryOverride => transactionCategoryOverride.CategoryId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Transfer>(entity =>
        {
            entity.ToTable("Transfers");
            entity.HasKey(transfer => transfer.Id);
            entity.Property(transfer => transfer.RecipientName).IsRequired().HasMaxLength(200);
            entity.Property(transfer => transfer.RecipientIban).IsRequired().HasMaxLength(50)
                .HasColumnName("RecipientIBAN");
            entity.Property(transfer => transfer.RecipientBankName).HasMaxLength(200);
            entity.Property(transfer => transfer.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(transfer => transfer.Currency).IsRequired().HasMaxLength(10);
            entity.Property(transfer => transfer.ConvertedAmount).HasColumnType("decimal(18,2)");
            entity.Property(transfer => transfer.ExchangeRate).HasColumnType("decimal(18,6)");
            entity.Property(transfer => transfer.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(transfer => transfer.Reference).HasMaxLength(200);
            entity.Property(transfer => transfer.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(transfer => transfer.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(transfer => transfer.User).WithMany().HasForeignKey(transfer => transfer.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(transfer => transfer.SourceAccount).WithMany().HasForeignKey(transfer => transfer.SourceAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(transfer => transfer.Transaction).WithMany().HasForeignKey(transfer => transfer.TransactionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Biller>(entity =>
        {
            entity.ToTable("Biller");
            entity.HasKey(biller => biller.Id);
            entity.Property(biller => biller.Name).IsRequired().HasMaxLength(200);
            entity.HasIndex(biller => biller.Name).IsUnique();
            entity.Property(biller => biller.Category).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(biller => biller.LogoUrl).HasMaxLength(500);
            entity.Property(biller => biller.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<BillPayment>(entity =>
        {
            entity.ToTable("BillPayment");
            entity.HasKey(billPayment => billPayment.Id);
            entity.Property(billPayment => billPayment.BillerReference).IsRequired().HasMaxLength(200);
            entity.Property(billPayment => billPayment.Amount).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(billPayment => billPayment.Fee).HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(billPayment => billPayment.ReceiptNumber).IsRequired().HasMaxLength(100);
            entity.Property(billPayment => billPayment.Status).IsRequired().HasConversion<string>().HasMaxLength(50);
            entity.Property(billPayment => billPayment.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(billPayment => billPayment.User).WithMany().HasForeignKey(billPayment => billPayment.UserId);
            entity.HasOne(billPayment => billPayment.SourceAccount).WithMany().HasForeignKey(billPayment => billPayment.SourceAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(billPayment => billPayment.Biller).WithMany().HasForeignKey(billPayment => billPayment.BillerId);
            entity.HasOne(billPayment => billPayment.Transaction).WithMany().HasForeignKey(billPayment => billPayment.TransactionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<SavedBiller>(entity =>
        {
            entity.ToTable("SavedBiller");
            entity.HasKey(savedBiller => savedBiller.Id);
            entity.Property(savedBiller => savedBiller.Nickname).HasMaxLength(200);
            entity.Property(savedBiller => savedBiller.DefaultReference).HasMaxLength(200);
            entity.Property(savedBiller => savedBiller.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(savedBiller => savedBiller.User).WithMany().HasForeignKey(savedBiller => savedBiller.UserId);
            entity.HasOne(savedBiller => savedBiller.Biller).WithMany().HasForeignKey(savedBiller => savedBiller.BillerId);
        });

        modelBuilder.Entity<ForexTransaction>(entity =>
        {
            entity.ToTable("ExchangeTransaction");
            entity.HasKey(exchangeTransaction => exchangeTransaction.Id);
            entity.Property(exchangeTransaction => exchangeTransaction.SourceCurrency).IsRequired().HasMaxLength(3);
            entity.Property(exchangeTransaction => exchangeTransaction.TargetCurrency).IsRequired().HasMaxLength(3);
            entity.Property(exchangeTransaction => exchangeTransaction.SourceAmount).IsRequired()
                .HasColumnType("decimal(18,2)");
            entity.Property(exchangeTransaction => exchangeTransaction.TargetAmount).IsRequired()
                .HasColumnType("decimal(18,2)");
            entity.Property(exchangeTransaction => exchangeTransaction.ExchangeRate).IsRequired()
                .HasColumnType("decimal(18,6)");
            entity.Property(exchangeTransaction => exchangeTransaction.Commission).IsRequired()
                .HasColumnType("decimal(18,2)");
            entity.Property(exchangeTransaction => exchangeTransaction.Status).IsRequired().HasConversion<string>()
                .HasMaxLength(20);
            entity.Property(exchangeTransaction => exchangeTransaction.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(exchangeTransaction => exchangeTransaction.User).WithMany().HasForeignKey(exchangeTransaction => exchangeTransaction.UserId);
            entity.HasOne(exchangeTransaction => exchangeTransaction.SourceAccount).WithMany()
                .HasForeignKey(exchangeTransaction => exchangeTransaction.SourceAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(exchangeTransaction => exchangeTransaction.TargetAccount).WithMany()
                .HasForeignKey(exchangeTransaction => exchangeTransaction.TargetAccountId)
                .OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(exchangeTransaction => exchangeTransaction.Transaction).WithMany()
                .HasForeignKey(exchangeTransaction => exchangeTransaction.TransactionId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<RateAlert>(entity =>
        {
            entity.ToTable("RateAlert");
            entity.HasKey(rateAlert => rateAlert.Id);
            entity.Property(rateAlert => rateAlert.BaseCurrency).IsRequired().HasMaxLength(3);
            entity.Property(rateAlert => rateAlert.TargetCurrency).IsRequired().HasMaxLength(3);
            entity.Property(rateAlert => rateAlert.TargetRate).IsRequired().HasColumnType("decimal(18,6)");
            entity.Property(rateAlert => rateAlert.IsTriggered).HasDefaultValue(false);
            entity.Property(rateAlert => rateAlert.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasOne(rateAlert => rateAlert.User).WithMany().HasForeignKey(rateAlert => rateAlert.UserId);
        });

        modelBuilder.ApplyConfiguration(new RecurringPaymentConfiguration());
    }
}
