using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addnametoworkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_role_WorkFlows",
                table: "role_WorkFlows");

            migrationBuilder.RenameTable(
                name: "role_WorkFlows",
                newName: "Role_WorkFlows");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WorkFlow",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "WorkFlow",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Role_WorkFlows",
                table: "Role_WorkFlows",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Role_WorkFlows",
                table: "Role_WorkFlows");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WorkFlow");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "WorkFlow");

            migrationBuilder.RenameTable(
                name: "Role_WorkFlows",
                newName: "role_WorkFlows");

            migrationBuilder.AddPrimaryKey(
                name: "PK_role_WorkFlows",
                table: "role_WorkFlows",
                column: "Id");
        }
    }
}
