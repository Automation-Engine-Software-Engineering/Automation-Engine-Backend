using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addseeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Entity",
                columns: new[] { "Id", "Description", "PreviewName", "TableName" },
                values: new object[] { 1, "RelationLists", "RelationLists", "RelationLists" });

            migrationBuilder.InsertData(
                table: "Property",
                columns: new[] { "Id", "DefaultErrorMessage", "DefaultValue", "Description", "EntityId", "IconClass", "IsRequiredErrorMessage", "PreviewName", "PropertyName", "ToolType", "Type" },
                values: new object[,]
                {
                    { 1, null, null, "RelationId", 1, null, null, "RelationId", "RelationId", null, 1 },
                    { 2, null, null, "Element2", 1, null, null, "Element2", "Element2", null, 1 },
                    { 3, null, null, "Element1", 1, null, null, "Element1", "Element1", null, 1 },
                    { 4, null, null, "WorkflowUserId", 1, null, null, "WorkflowUserId", "WorkflowUserId", null, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Property",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Entity",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
