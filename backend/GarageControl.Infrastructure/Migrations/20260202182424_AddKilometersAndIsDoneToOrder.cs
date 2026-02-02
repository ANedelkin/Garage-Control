using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddKilometersAndIsDoneToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDone",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Kilometers",
                table: "Orders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDone",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Kilometers",
                table: "Orders");
        }
    }
}
