using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumen.Modules.FocusStats.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "focusstats");

            migrationBuilder.CreateTable(
                name: "Activities",
                schema: "focusstats",
                columns: table => new
                {
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Device = table.Column<string>(type: "character varying", nullable: false, defaultValue: "Unknown"),
                    SecondsDuration = table.Column<int>(type: "integer", nullable: false),
                    AppOrExe = table.Column<string>(type: "character varying", nullable: false),
                    Name = table.Column<string>(type: "character varying", nullable: false),
                    Tags = table.Column<List<string>>(type: "character varying[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Activities", x => new { x.Device, x.StartTime });
                });

            migrationBuilder.CreateTable(
                name: "CleaningRules",
                schema: "focusstats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Regex = table.Column<string>(type: "character varying", nullable: false),
                    Target = table.Column<string>(type: "character varying", nullable: false),
                    Tests = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CleaningRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TaggingRules",
                schema: "focusstats",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Regex = table.Column<string>(type: "character varying", nullable: false),
                    Target = table.Column<string>(type: "character varying", nullable: false),
                    Tests = table.Column<string>(type: "character varying", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaggingRules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Activities_StartTime_SecondsDuration_AppOrExe_Name_Device",
                schema: "focusstats",
                table: "Activities",
                columns: new[] { "StartTime", "SecondsDuration", "AppOrExe", "Name", "Device" });

            migrationBuilder.CreateIndex(
                name: "IX_CleaningRules_Regex",
                schema: "focusstats",
                table: "CleaningRules",
                column: "Regex",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TaggingRules_Regex",
                schema: "focusstats",
                table: "TaggingRules",
                column: "Regex",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Activities",
                schema: "focusstats");

            migrationBuilder.DropTable(
                name: "CleaningRules",
                schema: "focusstats");

            migrationBuilder.DropTable(
                name: "TaggingRules",
                schema: "focusstats");
        }
    }
}
