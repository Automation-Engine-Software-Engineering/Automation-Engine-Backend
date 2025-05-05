using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addrelation2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Entity_EntityRelation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entity_EntityRelation_EntityId",
                table: "Entity_EntityRelation",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entity_EntityRelation_Entity_EntityId",
                table: "Entity_EntityRelation",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entity_EntityRelation_Entity_EntityId",
                table: "Entity_EntityRelation");

            migrationBuilder.DropIndex(
                name: "IX_Entity_EntityRelation_EntityId",
                table: "Entity_EntityRelation");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Entity_EntityRelation");
        }
    }
}
