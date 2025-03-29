using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class seedworkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WorkFlow",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Form Builder", "Form Builder" },
                    { 2, "Workflow Builder", "Workflow Builder" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "WorkFlow",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WorkFlow",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
