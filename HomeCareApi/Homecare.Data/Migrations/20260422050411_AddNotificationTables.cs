using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    CustomerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentMethodValue = table.Column<int>(type: "integer", nullable: false),
                    SlotDate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SlotTime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_admin_notifications_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "admin_notification_reads",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false),
                    AdminId = table.Column<int>(type: "integer", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_notification_reads", x => new { x.NotificationId, x.AdminId });
                    table.ForeignKey(
                        name: "FK_admin_notification_reads_admin_AdminId",
                        column: x => x.AdminId,
                        principalTable: "admin",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_admin_notification_reads_admin_notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "admin_notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_notification_reads_AdminId",
                table: "admin_notification_reads",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_notification_reads_AdminId_NotificationId",
                table: "admin_notification_reads",
                columns: new[] { "AdminId", "NotificationId" });

            migrationBuilder.CreateIndex(
                name: "IX_admin_notifications_BookingId",
                table: "admin_notifications",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_admin_notifications_CreatedAt",
                table: "admin_notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_admin_notifications_CustomerId",
                table: "admin_notifications",
                column: "CustomerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_notification_reads");

            migrationBuilder.DropTable(
                name: "admin_notifications");
        }
    }
}
