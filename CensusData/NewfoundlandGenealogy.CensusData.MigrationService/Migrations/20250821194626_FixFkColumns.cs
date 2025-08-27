using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewfoundlandGenealogy.CensusData.MigrationService.Migrations
{
    /// <inheritdoc />
    public partial class FixFkColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_census_transcriptions_census_districts_census_district_cens",
                table: "census_transcriptions");

            migrationBuilder.DropIndex(
                name: "ix_census_transcriptions_census_district_census_id_census_dist",
                table: "census_transcriptions");

            migrationBuilder.DropColumn(
                name: "census_district_census_id",
                table: "census_transcriptions");

            migrationBuilder.DropColumn(
                name: "census_district_id",
                table: "census_transcriptions");

            migrationBuilder.AddForeignKey(
                name: "fk_census_transcriptions_census_districts_census_id_district_id",
                table: "census_transcriptions",
                columns: new[] { "census_id", "district_id" },
                principalTable: "census_districts",
                principalColumns: new[] { "census_id", "id" },
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_census_transcriptions_census_districts_census_id_district_id",
                table: "census_transcriptions");

            migrationBuilder.AddColumn<string>(
                name: "census_district_census_id",
                table: "census_transcriptions",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "census_district_id",
                table: "census_transcriptions",
                type: "character varying(100)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_census_transcriptions_census_district_census_id_census_dist",
                table: "census_transcriptions",
                columns: new[] { "census_district_census_id", "census_district_id" });

            migrationBuilder.AddForeignKey(
                name: "fk_census_transcriptions_census_districts_census_district_cens",
                table: "census_transcriptions",
                columns: new[] { "census_district_census_id", "census_district_id" },
                principalTable: "census_districts",
                principalColumns: new[] { "census_id", "id" });
        }
    }
}
