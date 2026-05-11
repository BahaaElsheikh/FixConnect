using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FixConnect.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewDimensionsAndJobLaborCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccuracyRating",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CommitmentRating",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PriceRating",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SuggestWorker",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LaborCost",
                table: "Jobs",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccuracyRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CommitmentRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "PriceRating",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "SuggestWorker",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "LaborCost",
                table: "Jobs");
        }
    }
}
