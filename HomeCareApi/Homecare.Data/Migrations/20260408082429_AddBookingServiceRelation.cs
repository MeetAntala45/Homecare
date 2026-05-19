using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingServiceRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_bookings_ServiceId",
                table: "bookings",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_services_ServiceId",
                table: "bookings",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_services_ServiceId",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "IX_bookings_ServiceId",
                table: "bookings");
        }
    }
}
