using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewfoundlandGenealogy.CensusData.MigrationService.Migrations
{
    /// <inheritdoc />
    public partial class IncreaseIdColumnSizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "censuses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "census_district_id",
                table: "census_transcriptions",
                type: "character varying(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "census_district_census_id",
                table: "census_transcriptions",
                type: "character varying(100)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "census_transcriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "district_id",
                table: "census_transcriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "census_id",
                table: "census_transcriptions",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "census_districts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);

            migrationBuilder.AlterColumn<string>(
                name: "census_id",
                table: "census_districts",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldMaxLength: 25);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "censuses",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "census_district_id",
                table: "census_transcriptions",
                type: "character varying(25)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "census_district_census_id",
                table: "census_transcriptions",
                type: "character varying(25)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "census_transcriptions",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "district_id",
                table: "census_transcriptions",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "census_id",
                table: "census_transcriptions",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "id",
                table: "census_districts",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "census_id",
                table: "census_districts",
                type: "character varying(25)",
                maxLength: 25,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
