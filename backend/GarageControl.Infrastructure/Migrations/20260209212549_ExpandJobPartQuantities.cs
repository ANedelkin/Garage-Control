using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ExpandJobPartQuantities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AlterColumn<double>(
                name: "Quantity",
                table: "Parts",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "MinimumQuantity",
                table: "Parts",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "AvailabilityBalance",
                table: "Parts",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<double>(
                name: "PlannedQuantity",
                table: "JobParts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RequestedQuantity",
                table: "JobParts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "SentQuantity",
                table: "JobParts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "UsedQuantity",
                table: "JobParts",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            // Copy data from old Quantity column
            migrationBuilder.Sql("UPDATE JobParts SET PlannedQuantity = Quantity, SentQuantity = Quantity, UsedQuantity = Quantity, RequestedQuantity = Quantity");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "JobParts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlannedQuantity",
                table: "JobParts");

            migrationBuilder.DropColumn(
                name: "RequestedQuantity",
                table: "JobParts");

            migrationBuilder.DropColumn(
                name: "SentQuantity",
                table: "JobParts");

            migrationBuilder.DropColumn(
                name: "UsedQuantity",
                table: "JobParts");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "Parts",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "MinimumQuantity",
                table: "Parts",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<int>(
                name: "AvailabilityBalance",
                table: "Parts",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "JobParts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
