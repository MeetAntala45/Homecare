using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCouponConditions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConditionType",
                table: "coupon_conditions");

            migrationBuilder.AddColumn<int>(
                name: "ConditionTypeId",
                table: "coupon_conditions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "coupon_condition_types",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ContextKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InputType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultOperator = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DefaultFailBehaviour = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "disable"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedBy = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ModifiedBy = table.Column<int>(type: "integer", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_condition_types", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coupon_conditions_ConditionTypeId",
                table: "coupon_conditions",
                column: "ConditionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_coupon_conditions_CouponId_ConditionTypeId",
                table: "coupon_conditions",
                columns: new[] { "CouponId", "ConditionTypeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_coupon_condition_types_ContextKey",
                table: "coupon_condition_types",
                column: "ContextKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_coupon_conditions_coupon_condition_types_ConditionTypeId",
                table: "coupon_conditions",
                column: "ConditionTypeId",
                principalTable: "coupon_condition_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_coupon_conditions_coupon_condition_types_ConditionTypeId",
                table: "coupon_conditions");

            migrationBuilder.DropTable(
                name: "coupon_condition_types");

            migrationBuilder.DropIndex(
                name: "IX_coupon_conditions_ConditionTypeId",
                table: "coupon_conditions");

            migrationBuilder.DropIndex(
                name: "IX_coupon_conditions_CouponId_ConditionTypeId",
                table: "coupon_conditions");

            migrationBuilder.DropColumn(
                name: "ConditionTypeId",
                table: "coupon_conditions");

            migrationBuilder.AddColumn<string>(
                name: "ConditionType",
                table: "coupon_conditions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}
