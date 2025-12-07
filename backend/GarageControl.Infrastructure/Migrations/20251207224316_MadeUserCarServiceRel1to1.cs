using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MadeUserCarServiceRel1to1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CarServices_BossId",
                table: "CarServices");

            migrationBuilder.AddColumn<string>(
                name: "CarServiceId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

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

            migrationBuilder.DropColumn(
                name: "CarServiceId",
                table: "AspNetUsers");

            migrationBuilder.CreateIndex(
                name: "IX_CarServices_BossId",
                table: "CarServices",
                column: "BossId");
        }
    }
}
