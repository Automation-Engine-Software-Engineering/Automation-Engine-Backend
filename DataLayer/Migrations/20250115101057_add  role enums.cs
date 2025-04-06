using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addroleenums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "WorkFlow",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 3, "مدیریت", "مدیریت" });

            migrationBuilder.InsertData(
                table: "Role_WorkFlows",
                columns: new[] { "Id", "RoleId", "WorkFlowId" },
                values: new object[] { 3, 1, 3 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role_WorkFlows",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WorkFlow",
                keyColumn: "Id",
                keyValue: 3);
        }
    }
}
