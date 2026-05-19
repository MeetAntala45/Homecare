using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemNotifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "admin_system_notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FromPartnerId = table.Column<int>(type: "integer", nullable: true),
                    FromPartnerName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_admin_system_notifications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "partner_system_notifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    ReferenceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partner_system_notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_partner_system_notifications_service_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "service_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_admin_system_notifications_CreatedAt",
                table: "admin_system_notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_admin_system_notifications_IsRead",
                table: "admin_system_notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_admin_system_notifications_Type",
                table: "admin_system_notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_partner_system_notifications_CreatedAt",
                table: "partner_system_notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_partner_system_notifications_PartnerId",
                table: "partner_system_notifications",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_partner_system_notifications_PartnerId_IsRead",
                table: "partner_system_notifications",
                columns: new[] { "PartnerId", "IsRead" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "admin_system_notifications");

            migrationBuilder.DropTable(
                name: "partner_system_notifications");
        }
    }
}
