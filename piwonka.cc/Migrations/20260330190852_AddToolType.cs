using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Piwonka.CC.Migrations
{
    /// <inheritdoc />
    public partial class AddToolType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ToolType",
                table: "PlanAdviceResults",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ToolType",
                table: "PlanAdviceResults");
        }
    }
}
