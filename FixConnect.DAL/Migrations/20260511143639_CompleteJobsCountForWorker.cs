using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CompleteJobsCountForWorker : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompletedJobsCount",
                table: "Workers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedJobsCount",
                table: "Workers");
        }
    }
}
