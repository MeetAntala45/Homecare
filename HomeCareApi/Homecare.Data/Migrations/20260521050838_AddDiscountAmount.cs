using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "WalletDiscountAmount",
                table: "bookings",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WalletDiscountAmount",
                table: "bookings");
        }
    }
}
