using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerNotificationState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerNotificationStates",
                columns: table => new
                {
                    CustomerId = table.Column<int>(type: "int", nullable: false),
                    LastSeenProposalsReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenJobs = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastSeenRequests = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotificationStates", x => x.CustomerId);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationStates_Users_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerNotificationStates");
        }
    }
}
