using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addrelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MenueElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenueType = table.Column<int>(type: "int", nullable: false),
                    ParentMenueElemntId = table.Column<int>(type: "int", nullable: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenueElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenueElements_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MenueElements_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RelationLists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Element1 = table.Column<int>(type: "int", nullable: false),
                    Element2 = table.Column<int>(type: "int", nullable: false),
                    RelationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelationLists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RelationLists_Entity_EntityRelation_RelationId",
                        column: x => x.RelationId,
                        principalTable: "Entity_EntityRelation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenueElements_RoleId",
                table: "MenueElements",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_MenueElements_WorkflowId",
                table: "MenueElements",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_RelationLists_RelationId",
                table: "RelationLists",
                column: "RelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MenueElements");

            migrationBuilder.DropTable(
                name: "RelationLists");
        }
    }
}
