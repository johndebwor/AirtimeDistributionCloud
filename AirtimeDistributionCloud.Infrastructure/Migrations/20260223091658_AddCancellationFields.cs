using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancellationFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "AirtimeTransfers",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "AirtimeTransfers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "AirtimeTransfers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancellationReason",
                table: "AirtimeDeposits",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CancelledByUserId",
                table: "AirtimeDeposits",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledDate",
                table: "AirtimeDeposits",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "AirtimeTransfers");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "AirtimeTransfers");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "AirtimeTransfers");

            migrationBuilder.DropColumn(
                name: "CancellationReason",
                table: "AirtimeDeposits");

            migrationBuilder.DropColumn(
                name: "CancelledByUserId",
                table: "AirtimeDeposits");

            migrationBuilder.DropColumn(
                name: "CancelledDate",
                table: "AirtimeDeposits");
        }
    }
}
