using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lumen.Modules.FocusStats.Data.Migrations
{
    /// <inheritdoc />
    public partial class MissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "Tags",
                schema: "focusstats",
                table: "TaggingRules",
                type: "character varying[]",
                nullable: false,
                defaultValue: new string[0]);

            migrationBuilder.AddColumn<string>(
                name: "Replacement",
                schema: "focusstats",
                table: "CleaningRules",
                type: "character varying",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tags",
                schema: "focusstats",
                table: "TaggingRules");

            migrationBuilder.DropColumn(
                name: "Replacement",
                schema: "focusstats",
                table: "CleaningRules");
        }
    }
}
