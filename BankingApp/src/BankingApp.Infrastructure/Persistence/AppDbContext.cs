namespace BankingApp.Infrastructure.Persistence;

using BankingApp.Domain.Aggregates.AccountAggregate;
using BankingApp.Domain.Aggregates.AccountAggregate.Entities;
using BankingApp.Domain.Aggregates.BeneficiaryAggregate;
using BankingApp.Domain.Aggregates.BillPaymentAggregate;
using BankingApp.Domain.Aggregates.ForexAggregate;
using BankingApp.Domain.Aggregates.IdentityAggregate;
using BankingApp.Domain.Aggregates.RateAlertAggregate;
using BankingApp.Domain.Aggregates.RecurringPaymentAggregate;
using BankingApp.Domain.Aggregates.SavedBillerAggregate;
using BankingApp.Domain.Aggregates.TransferAggregate;
using BankingApp.Domain.Aggregates.UserAggregate;
using BankingApp.Domain.ReferenceData.Billers;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<User> Users => Set<User>();

    public DbSet<IdentityAccount> IdentityAccounts => Set<IdentityAccount>();

    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

    public DbSet<BillPayment> BillPayments => Set<BillPayment>();

    public DbSet<Biller> Billers => Set<Biller>();

    public DbSet<ForexTransaction> ForexTransactions => Set<ForexTransaction>();

    public DbSet<RateAlert> RateAlerts => Set<RateAlert>();

    public DbSet<RecurringPayment> RecurringPayments => Set<RecurringPayment>();

    public DbSet<SavedBiller> SavedBillers => Set<SavedBiller>();

    public DbSet<Transfer> Transfers => Set<Transfer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
