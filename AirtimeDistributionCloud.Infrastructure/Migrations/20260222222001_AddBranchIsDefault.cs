using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchIsDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDefault",
                table: "Branches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDefault",
                table: "Branches");
        }
    }
}
