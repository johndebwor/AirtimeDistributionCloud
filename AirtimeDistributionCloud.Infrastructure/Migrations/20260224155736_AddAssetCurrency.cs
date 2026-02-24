using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Assets",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "SSP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Assets");
        }
    }
}
