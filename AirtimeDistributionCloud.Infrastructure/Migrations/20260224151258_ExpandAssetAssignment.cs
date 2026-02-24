using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandAssetAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssignmentType",
                table: "AssetAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Branch");

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "AssetAssignments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                table: "AssetAssignments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "AssetAssignments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AssetAssignments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OfficeName",
                table: "AssetAssignments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignmentType",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AssetAssignments");

            migrationBuilder.DropColumn(
                name: "OfficeName",
                table: "AssetAssignments");
        }
    }
}
