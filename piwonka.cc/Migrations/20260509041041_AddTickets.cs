using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piwonka.CC.Migrations
{
    /// <inheritdoc />
    public partial class AddTickets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tickets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Titel = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Beschreibung = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    MelderName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MelderEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IpAdresse = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    AdminNotiz = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ErstelltAm = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tickets");
        }
    }
}
