using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixformmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DefaultErrorMessage",
                table: "Property",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IconClass",
                table: "Property",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IsRequiredErrorMessage",
                table: "Property",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ToolType",
                table: "Property",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefaultErrorMessage",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "IconClass",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "IsRequiredErrorMessage",
                table: "Property");

            migrationBuilder.DropColumn(
                name: "ToolType",
                table: "Property");
        }
    }
}
