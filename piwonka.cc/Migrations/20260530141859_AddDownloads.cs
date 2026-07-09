using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piwonka.CC.Migrations
{
    /// <inheritdoc />
    public partial class AddDownloads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DownloadApps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Reihenfolge = table.Column<int>(type: "int", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false),
                    Language = table.Column<int>(type: "int", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadApps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DownloadDateien",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DownloadAppId = table.Column<int>(type: "int", nullable: false),
                    Plattform = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Hinweis = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    DateiPfad = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Dateigroesse = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VeroeffentlichtAm = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reihenfolge = table.Column<int>(type: "int", nullable: false),
                    Aktiv = table.Column<bool>(type: "bit", nullable: false),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DownloadDateien", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DownloadDateien_DownloadApps_DownloadAppId",
                        column: x => x.DownloadAppId,
                        principalTable: "DownloadApps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadDateien_DownloadAppId",
                table: "DownloadDateien",
                column: "DownloadAppId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DownloadDateien");

            migrationBuilder.DropTable(
                name: "DownloadApps");
        }
    }
}
