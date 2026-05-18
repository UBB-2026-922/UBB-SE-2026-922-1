namespace BankingApp.Infrastructure.Persistence.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518102200_CreateRecurringPayments")]
public partial class CreateRecurringPayments : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "RecurringPayments",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            UserId = table.Column<int>(type: "int", nullable: false),
                            BillerId = table.Column<int>(type: "int", nullable: false),
                            SourceAccountId = table.Column<int>(type: "int", nullable: false),
                            Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                            IsPayInFull = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                            Frequency = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                            StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                            EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                            NextExecutionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                            Status = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, defaultValue: "Active"),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table => table.PrimaryKey("PK_RecurringPayments", x => x.Id));
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "RecurringPayments");
    }
}
