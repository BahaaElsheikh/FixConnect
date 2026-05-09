using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddNotesToProposal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Proposals",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Proposals");
        }
    }
}
