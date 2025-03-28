using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node");

            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_NextNodeId",
                table: "Node");

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node",
                column: "LastNodeId",
                principalTable: "Node",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Node_Node_NextNodeId",
                table: "Node",
                column: "NextNodeId",
                principalTable: "Node",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_LastNodeId",
                table: "Node");

            migrationBuilder.DropForeignKey(
                name: "FK_Node_Node_NextNodeId",
                table: "Node");

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
    }
}
