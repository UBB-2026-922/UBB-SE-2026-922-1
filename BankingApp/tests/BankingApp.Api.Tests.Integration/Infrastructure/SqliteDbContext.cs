// <copyright file="SqliteDbContext.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the SqliteDbContext class used in API integration tests.
// </summary>

using BankingApp.Infrastructure.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BankingApp.Api.Tests.Integration.Infrastructure;

/// <summary>
///     A test-only subclass of <see cref="AppDatabaseContext" /> that targets SQLite.
///     Strips SQL Server-specific <c>defaultValueSql</c> expressions (e.g. <c>GETUTCDATE()</c>)
///     from the model so that <c>EnsureCreated()</c> generates valid SQLite DDL.
/// </summary>
public class SqliteDbContext : AppDatabaseContext
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="SqliteDbContext" /> class.
    /// </summary>
    /// <param name="options">The database context options.</param>
    public SqliteDbContext(DbContextOptions<AppDatabaseContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Strip all SQL Server-specific raw SQL default expressions.
        // SQLite cannot evaluate functions such as GETUTCDATE() at table-creation time,
        // so we remove them here and rely on the application layer to supply defaults.
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.GetDefaultValueSql() is not null)
                {
                    property.SetDefaultValueSql(null);
                }
            }
        }
    }
}
