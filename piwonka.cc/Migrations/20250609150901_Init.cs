using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Piwonka.CC.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kategorien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kategorien", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Seiten",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Inhalt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MetaDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    MetaKeywords = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IstVeroeffentlicht = table.Column<bool>(type: "bit", nullable: false),
                    ImMenuAnzeigen = table.Column<bool>(type: "bit", nullable: false),
                    Reihenfolge = table.Column<int>(type: "int", nullable: false),
                    Template = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BearbeitetAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    MenuGruppe = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MenuTitel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IstMenuGruppe = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Seiten", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Seiten_Seiten_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Seiten",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Inhalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BildUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IstVeroeffentlicht = table.Column<bool>(type: "bit", nullable: false),
                    KategorieId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Posts_Kategorien_KategorieId",
                        column: x => x.KategorieId,
                        principalTable: "Kategorien",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Kategorien",
                columns: new[] { "Id", "Beschreibung", "Name" },
                values: new object[,]
                {
                    { 1, "Allgemeine Beiträge", "Allgemein" },
                    { 2, "Beiträge über Technologie", "Technologie" },
                    { 3, "Beiträge über Lifestyle", "Lifestyle" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Posts_KategorieId",
                table: "Posts",
                column: "KategorieId");

            migrationBuilder.CreateIndex(
                name: "IX_Seiten_ParentId",
                table: "Seiten",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_Seiten_Slug",
                table: "Seiten",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Seiten");

            migrationBuilder.DropTable(
                name: "Kategorien");
        }
    }
}
