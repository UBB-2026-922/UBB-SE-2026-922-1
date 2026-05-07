#nullable disable

namespace BankingApp.Infrastructure.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddBillPaymentsFeature : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "BillPayment",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                SourceAccountId = table.Column<int>(type: "int", nullable: false),
                BillerId = table.Column<int>(type: "int", nullable: false),
                TransactionId = table.Column<int>(type: "int", nullable: true),
                BillerReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                ReceiptNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_BillPayment", x => x.Id);
                table.ForeignKey(
                    name: "FK_BillPayment_Account_SourceAccountId",
                    column: x => x.SourceAccountId,
                    principalTable: "Account",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_BillPayment_Biller_BillerId",
                    column: x => x.BillerId,
                    principalTable: "Biller",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_BillPayment_Transaction_TransactionId",
                    column: x => x.TransactionId,
                    principalTable: "Transaction",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.SetNull);
                table.ForeignKey(
                    name: "FK_BillPayment_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_BillerId",
            table: "BillPayment",
            column: "BillerId");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_SourceAccountId",
            table: "BillPayment",
            column: "SourceAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_TransactionId",
            table: "BillPayment",
            column: "TransactionId");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_UserId",
            table: "BillPayment",
            column: "UserId");

    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "BillPayment");

    }
}
