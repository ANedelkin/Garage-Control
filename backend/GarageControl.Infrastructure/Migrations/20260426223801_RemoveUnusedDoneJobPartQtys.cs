using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedDoneJobPartQtys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedQuantity",
                table: "CompletedJobParts");

            migrationBuilder.DropColumn(
                name: "RequestedQuantity",
                table: "CompletedJobParts");

            migrationBuilder.DropColumn(
                name: "SentQuantity",
                table: "CompletedJobParts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlannedQuantity",
                table: "CompletedJobParts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RequestedQuantity",
                table: "CompletedJobParts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SentQuantity",
                table: "CompletedJobParts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
