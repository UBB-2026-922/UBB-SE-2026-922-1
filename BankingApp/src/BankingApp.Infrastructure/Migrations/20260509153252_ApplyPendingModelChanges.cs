namespace BankingApp.Infrastructure.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class ApplyPendingModelChanges : Migration

    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillPayment_Account_SourceAccountId1",
                table: "BillPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPayment_Biller_BillerId1",
                table: "BillPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPayment_Transaction_TransactionId1",
                table: "BillPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_BillPayment_User_UserId1",
                table: "BillPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedBiller_Biller_BillerId1",
                table: "SavedBiller");

            migrationBuilder.DropForeignKey(
                name: "FK_SavedBiller_User_UserId1",
                table: "SavedBiller");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "FK_BillPayment_Transaction_TransactionId1",
                table: "BillPayment",
                column: "TransactionId1",
                principalTable: "Transaction",
                principalColumn: "Id");

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
                name: "FK_SavedBiller_User_UserId1",
                table: "SavedBiller",
                column: "UserId1",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
