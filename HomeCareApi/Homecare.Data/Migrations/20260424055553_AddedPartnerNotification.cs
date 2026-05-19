using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPartnerNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "partner_notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_partner_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_partner_notifications_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "partner_notification_reads",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "integer", nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_notification_reads", x => new { x.NotificationId, x.PartnerId });
                    table.ForeignKey(
                        name: "FK_partner_notification_reads_partner_notifications_Notificati~",
                        column: x => x.NotificationId,
                        principalTable: "partner_notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_partner_notification_reads_service_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "service_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_partner_notification_reads_PartnerId",
                table: "partner_notification_reads",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_notification_reads_PartnerId_NotificationId",
                table: "partner_notification_reads",
                columns: new[] { "PartnerId", "NotificationId" });

            migrationBuilder.CreateIndex(
                name: "IX_partner_notifications_BookingId",
                table: "partner_notifications",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_notifications_CreatedAt",
                table: "partner_notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_partner_notifications_CustomerId",
                table: "partner_notifications",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_notifications_PartnerId",
                table: "partner_notifications",
                column: "PartnerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "partner_notification_reads");

            migrationBuilder.DropTable(
                name: "partner_notifications");
        }
    }
}
