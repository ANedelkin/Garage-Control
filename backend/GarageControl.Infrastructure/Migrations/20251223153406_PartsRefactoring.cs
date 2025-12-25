using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PartsRefactoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_CarServices_CarServiceId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_PartsFolders_CarServices_CarServiceId",
                table: "PartsFolders");

            migrationBuilder.AlterColumn<string>(
                name: "ParentId",
                table: "PartsFolders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CarServiceId",
                table: "PartsFolders",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "ParentId",
                table: "Parts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "CarServiceId",
                table: "Parts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_CarServices_CarServiceId",
                table: "Parts",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartsFolders_CarServices_CarServiceId",
                table: "PartsFolders",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parts_CarServices_CarServiceId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_PartsFolders_CarServices_CarServiceId",
                table: "PartsFolders");

            migrationBuilder.AlterColumn<string>(
                name: "ParentId",
                table: "PartsFolders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CarServiceId",
                table: "PartsFolders",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ParentId",
                table: "Parts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CarServiceId",
                table: "Parts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_CarServices_CarServiceId",
                table: "Parts",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartsFolders_CarServices_CarServiceId",
                table: "PartsFolders",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
