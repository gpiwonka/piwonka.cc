using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piwonka.CC.Migrations
{
    /// <inheritdoc />
    public partial class AddLanguageFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Posts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Beschreibung",
                table: "Kategorien",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Kategorien",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: 1,
                column: "Language",
                value: "de");

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: 2,
                column: "Language",
                value: "de");

            migrationBuilder.UpdateData(
                table: "Kategorien",
                keyColumn: "Id",
                keyValue: 3,
                column: "Language",
                value: "de");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Language",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Kategorien");

            migrationBuilder.AlterColumn<string>(
                name: "Beschreibung",
                table: "Kategorien",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }
    }
}
