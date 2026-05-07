#nullable disable

namespace BankingApp.Infrastructure.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class DropOAuthLinkTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "OAuthLink");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "OAuthLink",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                LinkedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                Provider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ProviderEmail = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                ProviderUserId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                UserId = table.Column<int>(type: "int", nullable: false),
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_OAuthLink", x => x.Id);
                table.ForeignKey(
                    name: "FK_OAuthLink_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_OAuthLink_UserId",
            table: "OAuthLink",
            column: "UserId");
    }
}
