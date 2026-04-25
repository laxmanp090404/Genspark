using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class SeatUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_booking_seats_seat_id",
                schema: "public",
                table: "booking_seats");

            migrationBuilder.CreateIndex(
                name: "IX_booking_seats_seat_id",
                schema: "public",
                table: "booking_seats",
                column: "seat_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_booking_seats_seat_id",
                schema: "public",
                table: "booking_seats");

            migrationBuilder.CreateIndex(
                name: "IX_booking_seats_seat_id",
                schema: "public",
                table: "booking_seats",
                column: "seat_id",
                unique: true);
        }
    }
}
