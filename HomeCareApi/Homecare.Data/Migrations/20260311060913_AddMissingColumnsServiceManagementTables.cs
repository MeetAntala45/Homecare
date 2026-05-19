using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumnsServiceManagementTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "services");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                table: "services");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "services",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "IsDeleted",
                table: "service_images",
                newName: "is_deleted");

            migrationBuilder.RenameColumn(
                name: "DeletedOn",
                table: "service_images",
                newName: "modified_on");

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "service_images",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "modified_by",
                table: "service_images",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "created_by",
                table: "service_checklist",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "service_checklist",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "modified_by",
                table: "service_checklist",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "modified_on",
                table: "service_checklist",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "password_reset_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW()");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "service_images");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "service_images");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "service_checklist");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "service_checklist");

            migrationBuilder.DropColumn(
                name: "modified_by",
                table: "service_checklist");

            migrationBuilder.DropColumn(
                name: "modified_on",
                table: "service_checklist");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "services",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "is_deleted",
                table: "service_images",
                newName: "IsDeleted");

            migrationBuilder.RenameColumn(
                name: "modified_on",
                table: "service_images",
                newName: "DeletedOn");

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "services",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                table: "services",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "password_reset_tokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "CURRENT_TIMESTAMP");
        }
    }
}
