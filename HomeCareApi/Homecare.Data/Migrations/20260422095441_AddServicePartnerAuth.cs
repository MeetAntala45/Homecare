using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddServicePartnerAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_refresh_token_owner",
                table: "refresh_tokens");

            migrationBuilder.AddColumn<int>(
                name: "ServicePartnerId",
                table: "refresh_tokens",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartnerOtpVerifications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServicePartnerId = table.Column<int>(type: "integer", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    OtpCode = table.Column<string>(type: "text", nullable: false),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerOtpVerifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PartnerOtpVerifications_service_partners_ServicePartnerId",
                        column: x => x.ServicePartnerId,
                        principalTable: "service_partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_ServicePartnerId",
                table: "refresh_tokens",
                column: "ServicePartnerId");

            migrationBuilder.AddCheckConstraint(
                name: "chk_refresh_token_owner",
                table: "refresh_tokens",
                sql: "(\r\n                            (\"AdminId\" IS NOT NULL AND \"CustomerId\" IS NULL AND \"ServicePartnerId\" IS NULL)\r\n                            OR (\"AdminId\" IS NULL AND \"CustomerId\" IS NOT NULL AND \"ServicePartnerId\" IS NULL)\r\n                            OR (\"AdminId\" IS NULL AND \"CustomerId\" IS NULL AND \"ServicePartnerId\" IS NOT NULL)\r\n                            )");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerOtpVerifications_ServicePartnerId",
                table: "PartnerOtpVerifications",
                column: "ServicePartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_refresh_tokens_service_partners_ServicePartnerId",
                table: "refresh_tokens",
                column: "ServicePartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_refresh_tokens_service_partners_ServicePartnerId",
                table: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "PartnerOtpVerifications");

            migrationBuilder.DropIndex(
                name: "IX_refresh_tokens_ServicePartnerId",
                table: "refresh_tokens");

            migrationBuilder.DropCheckConstraint(
                name: "chk_refresh_token_owner",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "ServicePartnerId",
                table: "refresh_tokens");

            migrationBuilder.AddCheckConstraint(
                name: "chk_refresh_token_owner",
                table: "refresh_tokens",
                sql: "(\"AdminId\" IS NOT NULL AND \"CustomerId\" IS NULL) OR (\"AdminId\" IS NULL AND \"CustomerId\" IS NOT NULL)");
        }
    }
}
