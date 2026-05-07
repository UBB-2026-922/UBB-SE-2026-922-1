#nullable disable

namespace BankingApp.Infrastructure.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddRateAlertTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_Transaction_TransactionId",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_User_UserId",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_SavedBiller_User_UserId",
            table: "SavedBiller");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "Transaction",
            type: "nvarchar(max)",
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(30)",
            oldMaxLength: 30);

        migrationBuilder.AlterColumn<string>(
            name: "Nickname",
            table: "SavedBiller",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DefaultReference",
            table: "SavedBiller",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100,
            oldNullable: true);

        migrationBuilder.AddColumn<int>(
            name: "BillerId1",
            table: "SavedBiller",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "UserId1",
            table: "SavedBiller",
            type: "int",
            nullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "TransactionId",
            table: "BillPayment",
            type: "int",
            nullable: true,
            oldClrType: typeof(int),
            oldType: "int");

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            table: "BillPayment",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(20)",
            oldMaxLength: 20,
            oldDefaultValue: "Pending");

        migrationBuilder.AlterColumn<string>(
            name: "ReceiptNumber",
            table: "BillPayment",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "BillerReference",
            table: "BillPayment",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AddColumn<int>(
            name: "BillerId1",
            table: "BillPayment",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "SourceAccountId1",
            table: "BillPayment",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "TransactionId1",
            table: "BillPayment",
            type: "int",
            nullable: true);

        migrationBuilder.AddColumn<int>(
            name: "UserId1",
            table: "BillPayment",
            type: "int",
            nullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Biller",
            type: "nvarchar(200)",
            maxLength: 200,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "LogoUrl",
            table: "Biller",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(255)",
            oldMaxLength: 255,
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "ExchangeTransaction",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                SourceAccountId = table.Column<int>(type: "int", nullable: false),
                TargetAccountId = table.Column<int>(type: "int", nullable: false),
                TransactionId = table.Column<int>(type: "int", nullable: true),
                SourceCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                TargetCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                SourceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                TargetAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                RateLockedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ExchangeTransaction", x => x.Id);
                table.ForeignKey(
                    name: "FK_ExchangeTransaction_Account_SourceAccountId",
                    column: x => x.SourceAccountId,
                    principalTable: "Account",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ExchangeTransaction_Account_TargetAccountId",
                    column: x => x.TargetAccountId,
                    principalTable: "Account",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ExchangeTransaction_Transaction_TransactionId",
                    column: x => x.TransactionId,
                    principalTable: "Transaction",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_ExchangeTransaction_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "RateAlert",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                BaseCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                TargetCurrency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                TargetRate = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                IsTriggered = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                IsBuyAlert = table.Column<bool>(type: "bit", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_RateAlert", x => x.Id);
                table.ForeignKey(
                    name: "FK_RateAlert_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Transfers",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                UserId = table.Column<int>(type: "int", nullable: false),
                SourceAccountId = table.Column<int>(type: "int", nullable: false),
                TransactionId = table.Column<int>(type: "int", nullable: true),
                RecipientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                RecipientIBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                RecipientBankName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                ConvertedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                ExchangeRate = table.Column<decimal>(type: "decimal(18,6)", nullable: true),
                Fee = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                Reference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                EstimatedArrival = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Transfers", x => x.Id);
                table.ForeignKey(
                    name: "FK_Transfers_Account_SourceAccountId",
                    column: x => x.SourceAccountId,
                    principalTable: "Account",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Transfers_Transaction_TransactionId",
                    column: x => x.TransactionId,
                    principalTable: "Transaction",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_Transfers_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_SavedBiller_BillerId1",
            table: "SavedBiller",
            column: "BillerId1");

        migrationBuilder.CreateIndex(
            name: "IX_SavedBiller_UserId1",
            table: "SavedBiller",
            column: "UserId1");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_BillerId1",
            table: "BillPayment",
            column: "BillerId1");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_SourceAccountId1",
            table: "BillPayment",
            column: "SourceAccountId1");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_TransactionId1",
            table: "BillPayment",
            column: "TransactionId1");

        migrationBuilder.CreateIndex(
            name: "IX_BillPayment_UserId1",
            table: "BillPayment",
            column: "UserId1");

        migrationBuilder.CreateIndex(
            name: "IX_Biller_Name",
            table: "Biller",
            column: "Name",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ExchangeTransaction_SourceAccountId",
            table: "ExchangeTransaction",
            column: "SourceAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_ExchangeTransaction_TargetAccountId",
            table: "ExchangeTransaction",
            column: "TargetAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_ExchangeTransaction_TransactionId",
            table: "ExchangeTransaction",
            column: "TransactionId");

        migrationBuilder.CreateIndex(
            name: "IX_ExchangeTransaction_UserId",
            table: "ExchangeTransaction",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_RateAlert_UserId",
            table: "RateAlert",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_Transfers_SourceAccountId",
            table: "Transfers",
            column: "SourceAccountId");

        migrationBuilder.CreateIndex(
            name: "IX_Transfers_TransactionId",
            table: "Transfers",
            column: "TransactionId");

        migrationBuilder.CreateIndex(
            name: "IX_Transfers_UserId",
            table: "Transfers",
            column: "UserId");

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_Account_SourceAccountId1",
            table: "BillPayment",
            column: "SourceAccountId1",
            principalTable: "Account",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_Biller_BillerId1",
            table: "BillPayment",
            column: "BillerId1",
            principalTable: "Biller",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_Transaction_TransactionId",
            table: "BillPayment",
            column: "TransactionId",
            principalTable: "Transaction",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_Transaction_TransactionId1",
            table: "BillPayment",
            column: "TransactionId1",
            principalTable: "Transaction",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_User_UserId",
            table: "BillPayment",
            column: "UserId",
            principalTable: "User",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_User_UserId1",
            table: "BillPayment",
            column: "UserId1",
            principalTable: "User",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_SavedBiller_Biller_BillerId1",
            table: "SavedBiller",
            column: "BillerId1",
            principalTable: "Biller",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_SavedBiller_User_UserId",
            table: "SavedBiller",
            column: "UserId",
            principalTable: "User",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);

        migrationBuilder.AddForeignKey(
            name: "FK_SavedBiller_User_UserId1",
            table: "SavedBiller",
            column: "UserId1",
            principalTable: "User",
            principalColumn: "Id");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_Account_SourceAccountId1",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_Biller_BillerId1",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_Transaction_TransactionId",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_Transaction_TransactionId1",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_User_UserId",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_BillPayment_User_UserId1",
            table: "BillPayment");

        migrationBuilder.DropForeignKey(
            name: "FK_SavedBiller_Biller_BillerId1",
            table: "SavedBiller");

        migrationBuilder.DropForeignKey(
            name: "FK_SavedBiller_User_UserId",
            table: "SavedBiller");

        migrationBuilder.DropForeignKey(
            name: "FK_SavedBiller_User_UserId1",
            table: "SavedBiller");

        migrationBuilder.DropTable(
            name: "ExchangeTransaction");

        migrationBuilder.DropTable(
            name: "RateAlert");

        migrationBuilder.DropTable(
            name: "Transfers");

        migrationBuilder.DropIndex(
            name: "IX_SavedBiller_BillerId1",
            table: "SavedBiller");

        migrationBuilder.DropIndex(
            name: "IX_SavedBiller_UserId1",
            table: "SavedBiller");

        migrationBuilder.DropIndex(
            name: "IX_BillPayment_BillerId1",
            table: "BillPayment");

        migrationBuilder.DropIndex(
            name: "IX_BillPayment_SourceAccountId1",
            table: "BillPayment");

        migrationBuilder.DropIndex(
            name: "IX_BillPayment_TransactionId1",
            table: "BillPayment");

        migrationBuilder.DropIndex(
            name: "IX_BillPayment_UserId1",
            table: "BillPayment");

        migrationBuilder.DropIndex(
            name: "IX_Biller_Name",
            table: "Biller");

        migrationBuilder.DropColumn(
            name: "BillerId1",
            table: "SavedBiller");

        migrationBuilder.DropColumn(
            name: "UserId1",
            table: "SavedBiller");

        migrationBuilder.DropColumn(
            name: "BillerId1",
            table: "BillPayment");

        migrationBuilder.DropColumn(
            name: "SourceAccountId1",
            table: "BillPayment");

        migrationBuilder.DropColumn(
            name: "TransactionId1",
            table: "BillPayment");

        migrationBuilder.DropColumn(
            name: "UserId1",
            table: "BillPayment");

        migrationBuilder.AlterColumn<string>(
            name: "Type",
            table: "Transaction",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(max)");

        migrationBuilder.AlterColumn<string>(
            name: "Nickname",
            table: "SavedBiller",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "DefaultReference",
            table: "SavedBiller",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200,
            oldNullable: true);

        migrationBuilder.AlterColumn<int>(
            name: "TransactionId",
            table: "BillPayment",
            type: "int",
            nullable: false,
            defaultValue: 0,
            oldClrType: typeof(int),
            oldType: "int",
            oldNullable: true);

        migrationBuilder.AlterColumn<string>(
            name: "Status",
            table: "BillPayment",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Pending",
            oldClrType: typeof(string),
            oldType: "nvarchar(50)",
            oldMaxLength: 50);

        migrationBuilder.AlterColumn<string>(
            name: "ReceiptNumber",
            table: "BillPayment",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(100)",
            oldMaxLength: 100);

        migrationBuilder.AlterColumn<string>(
            name: "BillerReference",
            table: "BillPayment",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "Name",
            table: "Biller",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(200)",
            oldMaxLength: 200);

        migrationBuilder.AlterColumn<string>(
            name: "LogoUrl",
            table: "Biller",
            type: "nvarchar(255)",
            maxLength: 255,
            nullable: true,
            oldClrType: typeof(string),
            oldType: "nvarchar(500)",
            oldMaxLength: 500,
            oldNullable: true);

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_Transaction_TransactionId",
            table: "BillPayment",
            column: "TransactionId",
            principalTable: "Transaction",
            principalColumn: "Id",
            onDelete: ReferentialAction.SetNull);

        migrationBuilder.AddForeignKey(
            name: "FK_BillPayment_User_UserId",
            table: "BillPayment",
            column: "UserId",
            principalTable: "User",
            principalColumn: "Id");

        migrationBuilder.AddForeignKey(
            name: "FK_SavedBiller_User_UserId",
            table: "SavedBiller",
            column: "UserId",
            principalTable: "User",
            principalColumn: "Id");
    }
}
