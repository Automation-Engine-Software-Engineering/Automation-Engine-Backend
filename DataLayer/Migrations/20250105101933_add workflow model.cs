using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addworkflowmodel : Migration
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

            migrationBuilder.DropTable(
                name: "Edge");

            migrationBuilder.DropIndex(
                name: "IX_Node_entityId",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "entityId",
                table: "Node");

            migrationBuilder.RenameColumn(
                name: "formId",
                table: "Node",
                newName: "FormId");

            migrationBuilder.RenameIndex(
                name: "IX_Node_formId",
                table: "Node",
                newName: "IX_Node_FormId");

            migrationBuilder.AlterColumn<string>(
                name: "Icon",
                table: "Node",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<float>(
                name: "Height",
                table: "Node",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "LastNodeId",
                table: "Node",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NextNodeId",
                table: "Node",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Width",
                table: "Node",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateIndex(
                name: "IX_Node_LastNodeId",
                table: "Node",
                column: "LastNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_NextNodeId",
                table: "Node",
                column: "NextNodeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Form_FormId",
                table: "Node",
                column: "FormId",
                principalTable: "Form",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node",
                column: "LastNodeId",
                principalTable: "Node",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Node_NextNodeId",
                table: "Node",
                column: "NextNodeId",
                principalTable: "Node",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Form_FormId",
                table: "Node");

            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node");

            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_NextNodeId",
                table: "Node");

            migrationBuilder.DropIndex(
                name: "IX_Node_LastNodeId",
                table: "Node");

            migrationBuilder.DropIndex(
                name: "IX_Node_NextNodeId",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "LastNodeId",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "NextNodeId",
                table: "Node");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "Node");

            migrationBuilder.RenameColumn(
                name: "FormId",
                table: "Node",
                newName: "formId");

            migrationBuilder.RenameIndex(
                name: "IX_Node_FormId",
                table: "Node",
                newName: "IX_Node_formId");

            migrationBuilder.AlterColumn<int>(
                name: "Icon",
                table: "Node",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "entityId",
                table: "Node",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Edge",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceHandle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Target = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TargetHandle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkFlowId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Edge", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Edge_WorkFlow_WorkFlowId",
                        column: x => x.WorkFlowId,
                        principalTable: "WorkFlow",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Node_entityId",
                table: "Node",
                column: "entityId");

            migrationBuilder.CreateIndex(
                name: "IX_Edge_WorkFlowId",
                table: "Edge",
                column: "WorkFlowId");

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
    }
}
