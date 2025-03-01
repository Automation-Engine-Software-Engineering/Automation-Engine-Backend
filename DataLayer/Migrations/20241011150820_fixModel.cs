using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class fixModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Peroperty",
                newName: "PreviewName");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Entity",
                newName: "TableName");

            migrationBuilder.AddColumn<bool>(
                name: "AllowNull",
                table: "Peroperty",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "DefaultValue",
                table: "Peroperty",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PeropertyName",
                table: "Peroperty",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Form",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "BackgroundImgPath",
                table: "Form",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PreviewName",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowNull",
                table: "Peroperty");

            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "Peroperty");

            migrationBuilder.DropColumn(
                name: "PeropertyName",
                table: "Peroperty");

            migrationBuilder.DropColumn(
                name: "BackgroundImgPath",
                table: "Form");

            migrationBuilder.DropColumn(
                name: "PreviewName",
                table: "Entity");

            migrationBuilder.RenameColumn(
                name: "PreviewName",
                table: "Peroperty",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "Entity",
                newName: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Form",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Entity",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
