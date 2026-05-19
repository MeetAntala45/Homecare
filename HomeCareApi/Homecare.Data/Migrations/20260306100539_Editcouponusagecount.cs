using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class Editcouponusagecount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsageCount",
                table: "coupons",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsageCount",
                table: "coupons");
        }
    }
}
