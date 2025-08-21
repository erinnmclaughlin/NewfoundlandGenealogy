using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewfoundlandGenealogy.CensusData.MigrationService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "censuses",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ngb_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    census_year = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", maxLength: 2147483647, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_censuses", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "census_districts",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    census_id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ngb_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "text", maxLength: 2147483647, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_census_districts", x => new { x.census_id, x.id });
                    table.ForeignKey(
                        name: "fk_census_districts_censuses_census_id",
                        column: x => x.census_id,
                        principalTable: "censuses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "census_transcriptions",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    census_id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    district_id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ngb_url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    markdown_content = table.Column<string>(type: "text", maxLength: 2147483647, nullable: false),
                    notes = table.Column<string>(type: "text", maxLength: 2147483647, nullable: true),
                    census_district_census_id = table.Column<string>(type: "character varying(25)", nullable: true),
                    census_district_id = table.Column<string>(type: "character varying(25)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_census_transcriptions", x => new { x.census_id, x.district_id, x.id });
                    table.ForeignKey(
                        name: "fk_census_transcriptions_census_districts_census_district_cens",
                        columns: x => new { x.census_district_census_id, x.census_district_id },
                        principalTable: "census_districts",
                        principalColumns: new[] { "census_id", "id" });
                });

            migrationBuilder.CreateIndex(
                name: "ix_census_transcriptions_census_district_census_id_census_dist",
                table: "census_transcriptions",
                columns: new[] { "census_district_census_id", "census_district_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "census_transcriptions");

            migrationBuilder.DropTable(
                name: "census_districts");

            migrationBuilder.DropTable(
                name: "censuses");
        }
    }
}
