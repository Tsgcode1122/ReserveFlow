using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReserveFlow.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationApprovalAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNote",
                table: "Reservations",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReviewedAt",
                table: "Reservations",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewedById",
                table: "Reservations",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_ReviewedById",
                table: "Reservations",
                column: "ReviewedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_AspNetUsers_ReviewedById",
                table: "Reservations",
                column: "ReviewedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservations_AspNetUsers_ReviewedById",
                table: "Reservations");

            migrationBuilder.DropIndex(
                name: "IX_Reservations_ReviewedById",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ApprovalNote",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ReviewedById",
                table: "Reservations");
        }
    }
}
