#nullable disable

namespace BankingApp.Infrastructure.Migrations;

using Microsoft.EntityFrameworkCore.Migrations;

/// <summary>
/// Represents the rebased transfer migration.
/// </summary>
/// <remarks>
/// The transfer-related schema changes were already introduced by the prior rebased migration.
/// Keeping this migration as a no-op preserves migration history without reapplying the same schema.
/// </remarks>
public partial class AddTransferTable : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
