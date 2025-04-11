using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class firstmigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Entity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviewName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TableName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SizeWidth = table.Column<double>(type: "float", nullable: false),
                    SizeHeight = table.Column<double>(type: "float", nullable: true),
                    IsAutoHeight = table.Column<bool>(type: "bit", nullable: false),
                    BackgroundColor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BackgroundImgPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRepeatedImage = table.Column<bool>(type: "bit", nullable: false),
                    HtmlFormBody = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Form", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Salt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IP = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workflow",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviewName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsRequiredErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToolType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    EntityId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Property_Entity_EntityId",
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

            migrationBuilder.CreateTable(
                name: "Role_Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Node",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    X = table.Column<float>(type: "real", nullable: false),
                    Y = table.Column<float>(type: "real", nullable: false),
                    Width = table.Column<float>(type: "real", nullable: false),
                    Height = table.Column<float>(type: "real", nullable: false),
                    FormId = table.Column<int>(type: "int", nullable: true),
                    LastNodeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    NextNodeId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Node", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Node_Form_FormId",
                        column: x => x.FormId,
                        principalTable: "Form",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Node_Node_LastNodeId",
                        column: x => x.LastNodeId,
                        principalTable: "Node",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Node_Node_NextNodeId",
                        column: x => x.NextNodeId,
                        principalTable: "Node",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Node_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Role_Workflows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role_Workflows", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Role_Workflows_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Role_Workflows_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Workflow_User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkflowState = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    WorkflowId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workflow_User", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Workflow_User_Workflow_WorkflowId",
                        column: x => x.WorkflowId,
                        principalTable: "Workflow",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[] { 1, "مدیر سیستم", "Admin" });

            migrationBuilder.InsertData(
                table: "Workflow",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Form Builder", "Form Builder" },
                    { 2, "Workflow Builder", "Workflow Builder" }
                });

            migrationBuilder.InsertData(
                table: "Role_Workflows",
                columns: new[] { "Id", "RoleId", "WorkflowId" },
                values: new object[,]
                {
                    { 1, 1, 1 },
                    { 2, 1, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_EntityForm_FormsId",
                table: "EntityForm",
                column: "FormsId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_FormId",
                table: "Node",
                column: "FormId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_LastNodeId",
                table: "Node",
                column: "LastNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_NextNodeId",
                table: "Node",
                column: "NextNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Node_WorkflowId",
                table: "Node",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_EntityId",
                table: "Property",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Users_RoleId",
                table: "Role_Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Workflows_RoleId",
                table: "Role_Workflows",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Workflows_WorkflowId",
                table: "Role_Workflows",
                column: "WorkflowId");

            migrationBuilder.CreateIndex(
                name: "IX_Workflow_User_WorkflowId",
                table: "Workflow_User",
                column: "WorkflowId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntityForm");

            migrationBuilder.DropTable(
                name: "Node");

            migrationBuilder.DropTable(
                name: "Property");

            migrationBuilder.DropTable(
                name: "Role_Users");

            migrationBuilder.DropTable(
                name: "Role_Workflows");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "Workflow_User");

            migrationBuilder.DropTable(
                name: "Form");

            migrationBuilder.DropTable(
                name: "Entity");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Workflow");
        }
    }
}
