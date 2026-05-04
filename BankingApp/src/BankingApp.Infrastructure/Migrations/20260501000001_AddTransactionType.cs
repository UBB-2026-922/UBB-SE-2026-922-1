// <copyright file="20260501000001_AddTransactionType.cs" company="UBB-922">
// Copyright (c) UBB-922. All rights reserved.
// </copyright>
// <summary>
// Contains the AddTransactionType migration.
// </summary>
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The column already exists in databases created from the SQL schema scripts.
            // The IF NOT EXISTS guard makes this migration idempotent so it is safe to
            // apply to both freshly-seeded and script-initialised databases.
            migrationBuilder.Sql(@"
                IF NOT EXISTS (
                    SELECT 1
                    FROM   sys.columns
                    WHERE  object_id = OBJECT_ID(N'dbo.[Transaction]')
                      AND  name = N'Type'
                )
                BEGIN
                    ALTER TABLE [Transaction]
                        ADD [Type] nvarchar(30) NOT NULL CONSTRAINT DF_Transaction_Type DEFAULT '';
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (
                    SELECT 1
                    FROM   sys.columns
                    WHERE  object_id = OBJECT_ID(N'dbo.[Transaction]')
                      AND  name = N'Type'
                )
                BEGIN
                    ALTER TABLE [Transaction] DROP CONSTRAINT IF EXISTS DF_Transaction_Type;
                    ALTER TABLE [Transaction] DROP COLUMN [Type];
                END
            ");
        }
    }
}
