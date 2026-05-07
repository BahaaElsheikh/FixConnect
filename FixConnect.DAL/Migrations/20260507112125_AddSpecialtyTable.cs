using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialtyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Specialty",
                table: "Workers");

            migrationBuilder.AddColumn<int>(
                name: "SpecialtyId",
                table: "Workers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialtyId",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Specialties",
                columns: table => new
                {
                    SpecialtyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpecialtyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Specialties", x => x.SpecialtyId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workers_SpecialtyId",
                table: "Workers",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_SpecialtyId",
                table: "Requests",
                column: "SpecialtyId");

            migrationBuilder.CreateIndex(
                name: "IX_Specialties_SpecialtyName",
                table: "Specialties",
                column: "SpecialtyName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Specialties_SpecialtyId",
                table: "Requests",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Workers_Specialties_SpecialtyId",
                table: "Workers",
                column: "SpecialtyId",
                principalTable: "Specialties",
                principalColumn: "SpecialtyId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Specialties_SpecialtyId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Workers_Specialties_SpecialtyId",
                table: "Workers");

            migrationBuilder.DropTable(
                name: "Specialties");

            migrationBuilder.DropIndex(
                name: "IX_Workers_SpecialtyId",
                table: "Workers");

            migrationBuilder.DropIndex(
                name: "IX_Requests_SpecialtyId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Workers");

            migrationBuilder.DropColumn(
                name: "SpecialtyId",
                table: "Requests");

            migrationBuilder.AddColumn<string>(
                name: "Specialty",
                table: "Workers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
