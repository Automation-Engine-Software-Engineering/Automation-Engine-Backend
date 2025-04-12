using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class changeNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "BackgroundColor",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "BackgroundImgPath",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "IsAutoHeight",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "IsRepeatedImage",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "SizeHeight",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "SizeWidth",
                table: "Form");

            migrationBuilder.RenameColumn(
                name: "LastNodeId",
                table: "Node",
                newName: "PreviousNodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Node_LastNodeId",
                table: "Node",
                newName: "IX_Node_PreviousNodeId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Workflow",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Workflow",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "دبیرخانه", "دبیرخانه" });

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Node_PreviousNodeId",
                table: "Node",
                column: "PreviousNodeId",
                principalTable: "Node",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_PreviousNodeId",
                table: "Node");

            migrationBuilder.RenameColumn(
                name: "PreviousNodeId",
                table: "Node",
                newName: "LastNodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Node_PreviousNodeId",
                table: "Node",
                newName: "IX_Node_LastNodeId");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Workflow",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundColor",
                table: "Form",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImgPath",
                table: "Form",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoHeight",
                table: "Form",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRepeatedImage",
                table: "Form",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "SizeHeight",
                table: "Form",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SizeWidth",
                table: "Form",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.UpdateData(
                table: "Workflow",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "Description", "Name" },
                values: new object[] { "دبیرحانه", "دبیرحانه" });

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node",
                column: "LastNodeId",
                principalTable: "Node",
                principalColumn: "Id");
        }
    }
}
