using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    public partial class RenamedCarServiceToWorkshop : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "CarServices",
                newName: "Workshops");

            migrationBuilder.RenameColumn(
                name: "CarServiceId",
                table: "Workers",
                newName: "WorkshopId");

            migrationBuilder.RenameIndex(
                name: "IX_Workers_CarServiceId",
                table: "Workers",
                newName: "IX_Workers_WorkshopId");

            migrationBuilder.RenameColumn(
                name: "CarServiceId",
                table: "PartsFolders",
                newName: "WorkshopId");

            migrationBuilder.RenameIndex(
                name: "IX_PartsFolders_CarServiceId",
                table: "PartsFolders",
                newName: "IX_PartsFolders_WorkshopId");

            migrationBuilder.RenameColumn(
                name: "CarServiceId",
                table: "Parts",
                newName: "WorkshopId");

            migrationBuilder.RenameIndex(
                name: "IX_Parts_CarServiceId",
                table: "Parts",
                newName: "IX_Parts_WorkshopId");

            migrationBuilder.RenameColumn(
                name: "CarServiceId",
                table: "JobTypes",
                newName: "WorkshopId");

            migrationBuilder.RenameIndex(
                name: "IX_JobTypes_CarServiceId",
                table: "JobTypes",
                newName: "IX_JobTypes_WorkshopId");

            migrationBuilder.RenameColumn(
                name: "CarServiceId",
                table: "Clients",
                newName: "WorkshopId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_CarServiceId",
                table: "Clients",
                newName: "IX_Clients_WorkshopId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_Workshops_WorkshopId",
                table: "Clients",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTypes_Workshops_WorkshopId",
                table: "JobTypes",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_Workshops_WorkshopId",
                table: "Parts",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartsFolders_Workshops_WorkshopId",
                table: "PartsFolders",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Workshops_WorkshopId",
                table: "Workers",
                column: "WorkshopId",
                principalTable: "Workshops",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clients_Workshops_WorkshopId",
                table: "Clients");

            migrationBuilder.DropForeignKey(
                name: "FK_JobTypes_Workshops_WorkshopId",
                table: "JobTypes");

            migrationBuilder.DropForeignKey(
                name: "FK_Parts_Workshops_WorkshopId",
                table: "Parts");

            migrationBuilder.DropForeignKey(
                name: "FK_PartsFolders_Workshops_WorkshopId",
                table: "PartsFolders");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Workshops_WorkshopId",
                table: "Workers");

            migrationBuilder.RenameTable(
                name: "Workshops",
                newName: "CarServices");

            migrationBuilder.RenameColumn(
                name: "WorkshopId",
                table: "Workers",
                newName: "CarServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Workers_WorkshopId",
                table: "Workers",
                newName: "IX_Workers_CarServiceId");

            migrationBuilder.RenameColumn(
                name: "WorkshopId",
                table: "PartsFolders",
                newName: "CarServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_PartsFolders_WorkshopId",
                table: "PartsFolders",
                newName: "IX_PartsFolders_CarServiceId");

            migrationBuilder.RenameColumn(
                name: "WorkshopId",
                table: "Parts",
                newName: "CarServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Parts_WorkshopId",
                table: "Parts",
                newName: "IX_Parts_CarServiceId");

            migrationBuilder.RenameColumn(
                name: "WorkshopId",
                table: "JobTypes",
                newName: "CarServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_JobTypes_WorkshopId",
                table: "JobTypes",
                newName: "IX_JobTypes_CarServiceId");

            migrationBuilder.RenameColumn(
                name: "WorkshopId",
                table: "Clients",
                newName: "CarServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_Clients_WorkshopId",
                table: "Clients",
                newName: "IX_Clients_CarServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clients_CarServices_CarServiceId",
                table: "Clients",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JobTypes_CarServices_CarServiceId",
                table: "JobTypes",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Parts_CarServices_CarServiceId",
                table: "Parts",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PartsFolders_CarServices_CarServiceId",
                table: "PartsFolders",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_CarServices_CarServiceId",
                table: "Workers",
                column: "CarServiceId",
                principalTable: "CarServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
