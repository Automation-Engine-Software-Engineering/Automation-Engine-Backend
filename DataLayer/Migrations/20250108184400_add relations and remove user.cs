using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class addrelationsandremoveuser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkFlow_User_User_UserId",
                table: "WorkFlow_User");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_WorkFlow_User_UserId",
                table: "WorkFlow_User");

            migrationBuilder.CreateIndex(
                name: "IX_Role_WorkFlows_RoleId",
                table: "Role_WorkFlows",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_WorkFlows_WorkFlowId",
                table: "Role_WorkFlows",
                column: "WorkFlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Role_Users_RoleId",
                table: "Role_Users",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Role_Users_Roles_RoleId",
                table: "Role_Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Role_WorkFlows_Roles_RoleId",
                table: "Role_WorkFlows",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Role_WorkFlows_WorkFlow_WorkFlowId",
                table: "Role_WorkFlows",
                column: "WorkFlowId",
                principalTable: "WorkFlow",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Role_Users_Roles_RoleId",
                table: "Role_Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_WorkFlows_Roles_RoleId",
                table: "Role_WorkFlows");

            migrationBuilder.DropForeignKey(
                name: "FK_Role_WorkFlows_WorkFlow_WorkFlowId",
                table: "Role_WorkFlows");

            migrationBuilder.DropIndex(
                name: "IX_Role_WorkFlows_RoleId",
                table: "Role_WorkFlows");

            migrationBuilder.DropIndex(
                name: "IX_Role_WorkFlows_WorkFlowId",
                table: "Role_WorkFlows");

            migrationBuilder.DropIndex(
                name: "IX_Role_Users_RoleId",
                table: "Role_Users");

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkFlow_User_UserId",
                table: "WorkFlow_User",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkFlow_User_User_UserId",
                table: "WorkFlow_User",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
