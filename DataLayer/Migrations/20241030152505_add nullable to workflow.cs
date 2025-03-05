using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addnullabletoworkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Entity_entityId",
                table: "Node");

            migrationBuilder.DropForeignKey(
                name: "FK_Node_Form_formId",
                table: "Node");

            migrationBuilder.AlterColumn<int>(
                name: "formId",
                table: "Node",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "entityId",
                table: "Node",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Entity_entityId",
                table: "Node",
                column: "entityId",
                principalTable: "Entity",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Form_formId",
                table: "Node",
                column: "formId",
                principalTable: "Form",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Entity_entityId",
                table: "Node");

            migrationBuilder.DropForeignKey(
                name: "FK_Node_Form_formId",
                table: "Node");

            migrationBuilder.AlterColumn<int>(
                name: "formId",
                table: "Node",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "entityId",
                table: "Node",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Entity_entityId",
                table: "Node",
                column: "entityId",
                principalTable: "Entity",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Form_formId",
                table: "Node",
                column: "formId",
                principalTable: "Form",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
