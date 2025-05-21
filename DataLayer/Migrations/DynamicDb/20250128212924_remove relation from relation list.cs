using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations.DynamicDb
{
    /// <inheritdoc />
    public partial class removerelationfromrelationlist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RelationLists_Entity_EntityRelation_RelationId",
                table: "RelationLists");

            migrationBuilder.DropTable(
                name: "Entity_EntityRelation");

            migrationBuilder.DropIndex(
                name: "IX_RelationLists_RelationId",
                table: "RelationLists");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entity_EntityRelation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChildId = table.Column<int>(type: "int", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity_EntityRelation", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelationLists_RelationId",
                table: "RelationLists",
                column: "RelationId");

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
