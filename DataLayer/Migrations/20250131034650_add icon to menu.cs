using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addicontomenu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationList_Entity_EntityRelation_RelationId",
                table: "RelationList");

            migrationBuilder.DropIndex(
                name: "IX_RelationList_RelationId",
                table: "RelationList");

            migrationBuilder.AlterColumn<int>(
                name: "Element2",
                table: "RelationList",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Element1",
                table: "RelationList",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "Entity_EntityRelationId",
                table: "RelationList",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "icon",
                table: "MenuElements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelationList_Entity_EntityRelationId",
                table: "RelationList",
                column: "Entity_EntityRelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationList_Entity_EntityRelation_Entity_EntityRelationId",
                table: "RelationList",
                column: "Entity_EntityRelationId",
                principalTable: "Entity_EntityRelation",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationList_Entity_EntityRelation_Entity_EntityRelationId",
                table: "RelationList");

            migrationBuilder.DropIndex(
                name: "IX_RelationList_Entity_EntityRelationId",
                table: "RelationList");

            migrationBuilder.DropColumn(
                name: "Entity_EntityRelationId",
                table: "RelationList");

            migrationBuilder.DropColumn(
                name: "icon",
                table: "MenuElements");

            migrationBuilder.AlterColumn<int>(
                name: "Element2",
                table: "RelationList",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Element1",
                table: "RelationList",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelationList_RelationId",
                table: "RelationList",
                column: "RelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_RelationList_Entity_EntityRelation_RelationId",
                table: "RelationList",
                column: "RelationId",
                principalTable: "Entity_EntityRelation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
