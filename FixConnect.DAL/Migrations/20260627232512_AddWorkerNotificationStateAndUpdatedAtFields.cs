using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerNotificationStateAndUpdatedAtFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Proposals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Proposals",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Jobs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkerNotificationStates",
                columns: table => new
                {
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    LastSeenDirectRequests = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "'1900-01-01'"),
                    LastSeenProposals = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "'1900-01-01'"),
                    LastSeenJobs = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "'1900-01-01'"),
                    LastSeenWallet = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "'1900-01-01'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerNotificationStates", x => x.WorkerId);
                    table.ForeignKey(
                        name: "FK_WorkerNotificationStates_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerNotificationStates");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Proposals");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Jobs");
        }
    }
}
