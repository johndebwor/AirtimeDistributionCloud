using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDealerPhotoPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoPath",
                table: "Dealers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhotoPath",
                table: "Dealers");
        }
    }
}
