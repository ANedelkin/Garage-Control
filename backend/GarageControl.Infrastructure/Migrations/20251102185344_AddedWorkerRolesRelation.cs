using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedWorkerRolesRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarServiceId1",
                table: "Workers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RoleWorker",
                columns: table => new
                {
                    RolesId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleWorker", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleWorker_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleWorker_Workers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Workers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workers_CarServiceId1",
                table: "Workers",
                column: "CarServiceId1");

            migrationBuilder.CreateIndex(
                name: "IX_RoleWorker_UsersId",
                table: "RoleWorker",
                column: "UsersId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_CarServices_CarServiceId1",
                table: "Workers",
                column: "CarServiceId1",
                principalTable: "CarServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workers_CarServices_CarServiceId1",
                table: "Workers");

            migrationBuilder.DropTable(
                name: "RoleWorker");

            migrationBuilder.DropIndex(
                name: "IX_Workers_CarServiceId1",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "CarServiceId1",
                table: "Workers");
        }
    }
}
