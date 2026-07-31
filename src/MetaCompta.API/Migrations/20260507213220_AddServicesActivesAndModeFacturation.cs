using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MetaCompta.API.Migrations
{
    /// <inheritdoc />
    public partial class AddServicesActivesAndModeFacturation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModeFacturation",
                table: "LignesDocumentsCommerciaux",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServicesActivesJson",
                table: "ContratsCommerciaux",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModeFacturation",
                table: "LignesDocumentsCommerciaux");

            migrationBuilder.DropColumn(
                name: "ServicesActivesJson",
                table: "ContratsCommerciaux");
        }
    }
}
