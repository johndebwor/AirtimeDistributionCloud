using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferTransactionNumbers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TransactionNumber",
                table: "CashDeposits",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TransferNumber",
                table: "AirtimeTransfers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TransactionNumber",
                table: "CashDeposits");

            migrationBuilder.DropColumn(
                name: "TransferNumber",
                table: "AirtimeTransfers");
        }
    }
}
