using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addreltion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationLists_Entity_EntityRelation_RelationId",
                table: "RelationLists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RelationLists",
                table: "RelationLists");

            migrationBuilder.RenameTable(
                name: "RelationLists",
                newName: "RelationList");

            migrationBuilder.RenameIndex(
                name: "IX_RelationLists_RelationId",
                table: "RelationList",
                newName: "IX_RelationList_RelationId");

            migrationBuilder.AddColumn<int>(
                name: "WorkflowUserId",
                table: "RelationList",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelationList",
                table: "RelationList",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationList_Entity_EntityRelation_RelationId",
                table: "RelationList",
                column: "RelationId",
                principalTable: "Entity_EntityRelation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationList_Entity_EntityRelation_RelationId",
                table: "RelationList");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RelationList",
                table: "RelationList");

            migrationBuilder.DropColumn(
                name: "WorkflowUserId",
                table: "RelationList");

            migrationBuilder.RenameTable(
                name: "RelationList",
                newName: "RelationLists");

            migrationBuilder.RenameIndex(
                name: "IX_RelationList_RelationId",
                table: "RelationLists",
                newName: "IX_RelationLists_RelationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RelationLists",
                table: "RelationLists",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationLists_Entity_EntityRelation_RelationId",
                table: "RelationLists",
                column: "RelationId",
                principalTable: "Entity_EntityRelation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
