using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AirtimeDistributionCloud.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create ExpenseCategories table first
            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_Name",
                table: "ExpenseCategories",
                column: "Name",
                unique: true);

            // 2. Seed default categories so FK references are valid
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [ExpenseCategories] ON;
                INSERT INTO [ExpenseCategories] ([Id],[Name],[Description],[IsActive],[CreatedDate],[CreatedBy])
                VALUES
                    (1, N'Transport',        N'Vehicle fuel, transport, and logistics costs', 1, GETUTCDATE(), N'System'),
                    (2, N'Office Supplies',   N'Stationery, printer cartridges, and other office consumables', 1, GETUTCDATE(), N'System'),
                    (3, N'Utilities',         N'Electricity, water, internet, and phone bills', 1, GETUTCDATE(), N'System'),
                    (4, N'Maintenance',       N'Building and equipment maintenance and repairs', 1, GETUTCDATE(), N'System'),
                    (5, N'Other',             N'Miscellaneous expenses not covered by other categories', 1, GETUTCDATE(), N'System');
                SET IDENTITY_INSERT [ExpenseCategories] OFF;
            ");

            // 3. Add ExpenseCategoryId column to Expenses (default 5 = 'Other')
            migrationBuilder.AddColumn<int>(
                name: "ExpenseCategoryId",
                table: "Expenses",
                type: "int",
                nullable: false,
                defaultValue: 5);

            // 4. Map existing Category string values to the new FK
            migrationBuilder.Sql(@"
                UPDATE [Expenses] SET [ExpenseCategoryId] = 1 WHERE [Category] = N'Transport';
                UPDATE [Expenses] SET [ExpenseCategoryId] = 2 WHERE [Category] = N'OfficeSupplies';
                UPDATE [Expenses] SET [ExpenseCategoryId] = 3 WHERE [Category] = N'Utilities';
                UPDATE [Expenses] SET [ExpenseCategoryId] = 4 WHERE [Category] = N'Maintenance';
                UPDATE [Expenses] SET [ExpenseCategoryId] = 5 WHERE [Category] = N'Other';
            ");

            // 5. Drop the old Category string column
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Expenses");

            // 6. Create index and FK constraint (all data now references valid categories)
            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Expenses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
                UPDATE [Expenses] SET [Category] = N'Transport' WHERE [ExpenseCategoryId] = 1;
                UPDATE [Expenses] SET [Category] = N'OfficeSupplies' WHERE [ExpenseCategoryId] = 2;
                UPDATE [Expenses] SET [Category] = N'Utilities' WHERE [ExpenseCategoryId] = 3;
                UPDATE [Expenses] SET [Category] = N'Maintenance' WHERE [ExpenseCategoryId] = 4;
                UPDATE [Expenses] SET [Category] = N'Other' WHERE [ExpenseCategoryId] = 5;
            ");

            migrationBuilder.DropColumn(
                name: "ExpenseCategoryId",
                table: "Expenses");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");
        }
    }
}
