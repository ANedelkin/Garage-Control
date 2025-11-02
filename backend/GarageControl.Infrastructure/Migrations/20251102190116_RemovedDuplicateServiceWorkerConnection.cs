using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemovedDuplicateServiceWorkerConnection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workers_CarServices_CarServiceId1",
                table: "Workers");

            migrationBuilder.DropIndex(
                name: "IX_Workers_CarServiceId1",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "CarServiceId1",
                table: "Workers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarServiceId1",
                table: "Workers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workers_CarServiceId1",
                table: "Workers",
                column: "CarServiceId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_CarServices_CarServiceId1",
                table: "Workers",
                column: "CarServiceId1",
                principalTable: "CarServices",
                principalColumn: "Id");
        }
    }
}
