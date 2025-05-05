using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixbug2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenueElements_Workflow_WorkflowId",
                table: "MenueElements");

            migrationBuilder.AlterColumn<int>(
                name: "WorkflowId",
                table: "MenueElements",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_MenueElements_Workflow_WorkflowId",
                table: "MenueElements",
                column: "WorkflowId",
                principalTable: "Workflow",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MenueElements_Workflow_WorkflowId",
                table: "MenueElements");

            migrationBuilder.AlterColumn<int>(
                name: "WorkflowId",
                table: "MenueElements",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_MenueElements_Workflow_WorkflowId",
                table: "MenueElements",
                column: "WorkflowId",
                principalTable: "Workflow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
