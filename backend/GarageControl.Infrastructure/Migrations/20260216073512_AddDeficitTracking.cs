using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeficitTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HigherDeficitSeverityCount",
                table: "PartsFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LowerDeficitSeverityCount",
                table: "PartsFolders",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DeficitStatus",
                table: "Parts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HigherDeficitSeverityCount",
                table: "PartsFolders");

            migrationBuilder.DropColumn(
                name: "LowerDeficitSeverityCount",
                table: "PartsFolders");

            migrationBuilder.DropColumn(
                name: "DeficitStatus",
                table: "Parts");
        }
    }
}
