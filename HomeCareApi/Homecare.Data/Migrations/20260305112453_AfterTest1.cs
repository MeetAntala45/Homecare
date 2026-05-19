using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AfterTest1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "admin",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
            //         MobileNumber = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
            //         Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
            //         PasswordHash = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
            //         Role = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
            //         ProfileImage = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
            //         Address = table.Column<string>(type: "text", nullable: true),
            //         IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            //         CreatedBy = table.Column<int>(type: "integer", nullable: false),
            //         CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         ModifiedBy = table.Column<int>(type: "integer", nullable: true),
            //         ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_admin", x => x.Id);
            //     });

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CouponCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DiscountPct = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupons", x => x.Id);
                });

            // migrationBuilder.CreateTable(
            //     name: "ServiceTypes",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            //         ImagePath = table.Column<string>(type: "text", nullable: true),
            //         CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         CreatedBy = table.Column<int>(type: "integer", nullable: false),
            //         ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         ModifiedBy = table.Column<int>(type: "integer", nullable: false),
            //         IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
            //         IsActive = table.Column<bool>(type: "boolean", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_ServiceTypes", x => x.Id);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "password_reset_tokens",
            //     columns: table => new
            //     {
            //         id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         admin_id = table.Column<int>(type: "integer", nullable: false),
            //         token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
            //         expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         is_used = table.Column<bool>(type: "boolean", nullable: false),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_password_reset_tokens", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_password_reset_tokens_admin",
            //             column: x => x.admin_id,
            //             principalTable: "admin",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "refresh_tokens",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
            //         ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         IsRevoked = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
            //         CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         AdminId = table.Column<int>(type: "integer", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_refresh_tokens", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_refresh_tokens_admin_AdminId",
            //             column: x => x.AdminId,
            //             principalTable: "admin",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "Categories",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            //         ServiceTypeId = table.Column<int>(type: "integer", nullable: false),
            //         CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         CreatedBy = table.Column<int>(type: "integer", nullable: false),
            //         ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         ModifiedBy = table.Column<int>(type: "integer", nullable: false),
            //         IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
            //         IsActive = table.Column<bool>(type: "boolean", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Categories", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_Categories_ServiceTypes_ServiceTypeId",
            //             column: x => x.ServiceTypeId,
            //             principalTable: "ServiceTypes",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Restrict);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "SubCategories",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "integer", nullable: false)
            //             .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
            //         Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
            //         CategoryId = table.Column<int>(type: "integer", nullable: false),
            //         CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         CreatedBy = table.Column<int>(type: "integer", nullable: false),
            //         ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         ModifiedBy = table.Column<int>(type: "integer", nullable: false),
            //         IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
            //         IsActive = table.Column<bool>(type: "boolean", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_SubCategories", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_SubCategories_Categories_CategoryId",
            //             column: x => x.CategoryId,
            //             principalTable: "Categories",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Restrict);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_admin_Email",
            //     table: "admin",
            //     column: "Email",
            //     unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_Categories_ServiceTypeId_Name",
            //     table: "Categories",
            //     columns: new[] { "ServiceTypeId", "Name" },
            //     unique: true,
            //     filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_coupons_CouponCode",
                table: "coupons",
                column: "CouponCode",
                unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_password_reset_tokens_admin_id",
            //     table: "password_reset_tokens",
            //     column: "admin_id");

            // migrationBuilder.CreateIndex(
            //     name: "IX_refresh_tokens_AdminId",
            //     table: "refresh_tokens",
            //     column: "AdminId");

            // migrationBuilder.CreateIndex(
            //     name: "IX_refresh_tokens_Token",
            //     table: "refresh_tokens",
            //     column: "Token",
            //     unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_ServiceTypes_Name",
            //     table: "ServiceTypes",
            //     column: "Name",
            //     unique: true,
            //     filter: "\"IsDeleted\" = false");

            // migrationBuilder.CreateIndex(
            //     name: "IX_SubCategories_CategoryId_Name",
            //     table: "SubCategories",
            //     columns: new[] { "CategoryId", "Name" },
            //     unique: true,
            //     filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "admin");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "ServiceTypes");
        }
    }
}
