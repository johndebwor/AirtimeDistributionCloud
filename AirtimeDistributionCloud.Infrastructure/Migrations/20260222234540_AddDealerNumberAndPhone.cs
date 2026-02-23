using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerNumberAndPhone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DealerNumber",
                table: "Dealers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Dealers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Backfill existing dealers with auto-generated DealerNumber
            migrationBuilder.Sql(
                "UPDATE Dealers SET DealerNumber = 'DLR-' + RIGHT('0000' + CAST(Id AS NVARCHAR), 4) WHERE DealerNumber = ''");

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_DealerNumber",
                table: "Dealers",
                column: "DealerNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Dealers_DealerNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "DealerNumber",
                table: "Dealers");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Dealers");
        }
    }
}
