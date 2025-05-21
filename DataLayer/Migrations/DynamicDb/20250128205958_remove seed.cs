using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataLayer.Migrations.DynamicDb
{
    /// <inheritdoc />
    public partial class removeseed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Entity_EntityRelation_Entity_EntityId",
                table: "Entity_EntityRelation");

            migrationBuilder.DropTable(
                name: "EntityForm");

            migrationBuilder.DropTable(
                name: "EntityProperty");

            migrationBuilder.DropTable(
                name: "Form");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropIndex(
                name: "IX_Entity_EntityRelation_EntityId",
                table: "Entity_EntityRelation");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "Entity_EntityRelation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntityId",
                table: "Entity_EntityRelation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviewName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Form",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HtmlFormBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EntityProperty",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntityId = table.Column<int>(type: "int", nullable: false),
                    DefaultErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequiredErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviewName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToolType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityProperty", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EntityProperty_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EntityForm",
                columns: table => new
                {
                    EntitiesId = table.Column<int>(type: "int", nullable: false),
                    FormsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntityForm", x => new { x.EntitiesId, x.FormsId });
                    table.ForeignKey(
                        name: "FK_EntityForm_Entity_EntitiesId",
                        column: x => x.EntitiesId,
                        principalTable: "Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EntityForm_Form_FormsId",
                        column: x => x.FormsId,
                        principalTable: "Form",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Entity",
                columns: new[] { "Id", "Description", "PreviewName", "TableName" },
                values: new object[] { 1, "RelationLists", "RelationLists", "RelationLists" });

            migrationBuilder.InsertData(
                table: "EntityProperty",
                columns: new[] { "Id", "DefaultErrorMessage", "DefaultValue", "Description", "EntityId", "IconClass", "IsRequiredErrorMessage", "PreviewName", "PropertyName", "ToolType", "Type" },
                values: new object[,]
                {
                    { 1, null, null, "RelationId", 1, null, null, "RelationId", "RelationId", null, 1 },
                    { 2, null, null, "Element2", 1, null, null, "Element2", "Element2", null, 1 },
                    { 3, null, null, "Element1", 1, null, null, "Element1", "Element1", null, 1 },
                    { 4, null, null, "WorkflowUserId", 1, null, null, "WorkflowUserId", "WorkflowUserId", null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entity_EntityRelation_EntityId",
                table: "Entity_EntityRelation",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityForm_FormsId",
                table: "EntityForm",
                column: "FormsId");

            migrationBuilder.CreateIndex(
                name: "IX_EntityProperty_EntityId",
                table: "EntityProperty",
                column: "EntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Entity_EntityRelation_Entity_EntityId",
                table: "Entity_EntityRelation",
                column: "EntityId",
                principalTable: "Entity",
                principalColumn: "Id");
        }
    }
}
