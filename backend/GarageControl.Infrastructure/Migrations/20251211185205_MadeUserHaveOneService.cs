using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MadeUserHaveOneService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarServices_BossId",
                table: "CarServices");

            migrationBuilder.CreateIndex(
                name: "IX_CarServices_BossId",
                table: "CarServices",
                column: "BossId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarServices_BossId",
                table: "CarServices");

            migrationBuilder.CreateIndex(
                name: "IX_CarServices_BossId",
                table: "CarServices",
                column: "BossId");
        }
    }
}
