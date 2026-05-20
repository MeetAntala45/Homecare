using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralAndWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReferralCode",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferralUseCount",
                table: "customers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "customer_wallets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CustomerId = table.Column<int>(type: "integer", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customer_wallets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_customer_wallets_customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "referral_uses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ReferrerId = table.Column<int>(type: "integer", nullable: false),
                    RefereeId = table.Column<int>(type: "integer", nullable: false),
                    ReferralCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RewardBookingId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    RewardedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_referral_uses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_referral_uses_customers_RefereeId",
                        column: x => x.RefereeId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_referral_uses_customers_ReferrerId",
                        column: x => x.ReferrerId,
                        principalTable: "customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WalletId = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    ReferenceId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_wallet_transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_wallet_transactions_customer_wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "customer_wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_customers_ReferralCode",
                table: "customers",
                column: "ReferralCode",
                unique: true,
                filter: "\"ReferralCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_customer_wallets_CustomerId",
                table: "customer_wallets",
                column: "CustomerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referral_uses_RefereeId",
                table: "referral_uses",
                column: "RefereeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_referral_uses_ReferralCode",
                table: "referral_uses",
                column: "ReferralCode");

            migrationBuilder.CreateIndex(
                name: "IX_referral_uses_ReferrerId",
                table: "referral_uses",
                column: "ReferrerId");

            migrationBuilder.CreateIndex(
                name: "IX_wallet_transactions_WalletId",
                table: "wallet_transactions",
                column: "WalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "referral_uses");

            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropTable(
                name: "customer_wallets");

            migrationBuilder.DropIndex(
                name: "IX_customers_ReferralCode",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "ReferralCode",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "ReferralUseCount",
                table: "customers");
        }
    }
}
