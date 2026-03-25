using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BauFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class TaxRateANgebot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TaxRate",
                table: "Quotes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Quotes");
        }
    }
}
