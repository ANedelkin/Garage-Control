using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarageControl.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompletedOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompletedOrders",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WorkshopId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompletionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkshopName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkshopAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkshopPhone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkshopEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WorkshopRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarRegistrationNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kilometers = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedOrders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompletedJobs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompletedOrderId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    JobTypeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WorkerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MechanicName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LaborCost = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedJobs_CompletedOrders_CompletedOrderId",
                        column: x => x.CompletedOrderId,
                        principalTable: "CompletedOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompletedJobParts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompletedJobId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PartId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PlannedQuantity = table.Column<int>(type: "int", nullable: false),
                    SentQuantity = table.Column<int>(type: "int", nullable: false),
                    UsedQuantity = table.Column<int>(type: "int", nullable: false),
                    RequestedQuantity = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompletedJobParts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompletedJobParts_CompletedJobs_CompletedJobId",
                        column: x => x.CompletedJobId,
                        principalTable: "CompletedJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompletedJobParts_CompletedJobId",
                table: "CompletedJobParts",
                column: "CompletedJobId");

            migrationBuilder.CreateIndex(
                name: "IX_CompletedJobs_CompletedOrderId",
                table: "CompletedJobs",
                column: "CompletedOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompletedJobParts");

            migrationBuilder.DropTable(
                name: "CompletedJobs");

            migrationBuilder.DropTable(
                name: "CompletedOrders");
        }
    }
}
