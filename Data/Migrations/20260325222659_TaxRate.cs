using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BauFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class TaxRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuoteItems_Invoices_InvoiceId",
                table: "QuoteItems");

            migrationBuilder.DropIndex(
                name: "IX_QuoteItems_InvoiceId",
                table: "QuoteItems");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "QuoteItems");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Invoices");

            migrationBuilder.AddColumn<Guid>(
                name: "InvoiceId",
                table: "QuoteItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuoteItems_InvoiceId",
                table: "QuoteItems",
                column: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteItems_Invoices_InvoiceId",
                table: "QuoteItems",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id");
        }
    }
}
