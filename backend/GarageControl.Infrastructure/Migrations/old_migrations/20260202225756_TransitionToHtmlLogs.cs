using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TransitionToHtmlLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Action",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ActorName",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ActorTargetId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "ActorType",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "TargetName",
                table: "ActivityLogs");

            migrationBuilder.DropColumn(
                name: "TargetType",
                table: "ActivityLogs");

            migrationBuilder.AddColumn<string>(
                name: "MessageHtml",
                table: "ActivityLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageHtml",
                table: "ActivityLogs");

            migrationBuilder.AddColumn<string>(
                name: "Action",
                table: "ActivityLogs",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActorName",
                table: "ActivityLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ActorTargetId",
                table: "ActivityLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActorType",
                table: "ActivityLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TargetId",
                table: "ActivityLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetName",
                table: "ActivityLogs",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TargetType",
                table: "ActivityLogs",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
