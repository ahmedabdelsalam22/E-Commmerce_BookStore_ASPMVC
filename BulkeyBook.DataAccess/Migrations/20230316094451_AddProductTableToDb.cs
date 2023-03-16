using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.Migrations
{
    /// <inheritdoc />
    public partial class AddProductTableToDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "Title");

            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Products",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "ListPrice",
                table: "Products",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Products",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "ListPrice",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Products");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "Products",
                newName: "Name");
        }
    }
}
