using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BauFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationUserErweitern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InviteAcceptedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InviteSentAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInviteAccepted",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InviteAcceptedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "InviteSentAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsInviteAccepted",
                table: "AspNetUsers");
        }
    }
}
