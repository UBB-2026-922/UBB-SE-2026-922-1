namespace BankingApp.Infrastructure.Persistence.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260518101200_CreatePasswordResetTokens")]
public partial class CreatePasswordResetTokens : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
                        name: "PasswordResetTokens",
                        columns: table => new
                        {
                            Id = table.Column<int>(type: "int", nullable: false)
                                .Annotation("SqlServer:Identity", "1, 1"),
                            IdentityAccountId = table.Column<int>(type: "int", nullable: false),
                            TokenHash = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false),
                            ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                            UsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                            CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                        },
                        constraints: table =>
                        {
                            table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
                            table.ForeignKey(
                                name: "FK_PasswordResetTokens_IdentityAccounts_IdentityAccountId",
                                column: x => x.IdentityAccountId,
                                principalTable: "IdentityAccounts",
                                principalColumn: "Id",
                                onDelete: ReferentialAction.Cascade);
                        });

        migrationBuilder.CreateIndex(
                        name: "IX_PasswordResetTokens_IdentityAccountId",
                        table: "PasswordResetTokens",
                        column: "IdentityAccountId");

        migrationBuilder.CreateIndex(
                        name: "IX_PasswordResetTokens_TokenHash",
                        table: "PasswordResetTokens",
                        column: "TokenHash",
                        unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "PasswordResetTokens");
    }
}
