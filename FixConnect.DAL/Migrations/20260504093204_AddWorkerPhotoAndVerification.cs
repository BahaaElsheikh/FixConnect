using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkerPhotoAndVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkerVerifications",
                columns: table => new
                {
                    VerificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WorkerId = table.Column<int>(type: "int", nullable: false),
                    IdFrontImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdBackImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false, defaultValue: "Pending"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkerVerifications", x => x.VerificationId);
                    table.ForeignKey(
                        name: "FK_WorkerVerifications_Workers_WorkerId",
                        column: x => x.WorkerId,
                        principalTable: "Workers",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkerVerifications_WorkerId",
                table: "WorkerVerifications",
                column: "WorkerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkerVerifications");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Workers");
        }
    }
}
