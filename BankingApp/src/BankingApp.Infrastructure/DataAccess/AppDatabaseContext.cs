// <copyright file="AppDatabaseContext.cs" company="CtrlC CtrlV">
// Copyright (c) CtrlC CtrlV. All rights reserved.
// </copyright>
// <summary>
// Contains the AppDatabaseContext class.
// </summary>

using BankingApp.Domain.Entities;
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
    }
}