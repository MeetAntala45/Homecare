using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceImages_services_ServiceId",
                table: "ServiceImages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceImages",
                table: "ServiceImages");

            migrationBuilder.RenameTable(
                name: "ServiceImages",
                newName: "service_images");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "service_images",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "service_images",
                newName: "service_id");

            migrationBuilder.RenameColumn(
                name: "ImagePath",
                table: "service_images",
                newName: "image_path");

            migrationBuilder.RenameColumn(
                name: "CreatedOn",
                table: "service_images",
                newName: "created_on");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceImages_ServiceId",
                table: "service_images",
                newName: "IX_service_images_service_id");

            migrationBuilder.AlterColumn<string>(
                name: "image_path",
                table: "service_images",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_service_images",
                table: "service_images",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_service_images_services_service_id",
                table: "service_images",
                column: "service_id",
                principalTable: "services",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_service_images_services_service_id",
                table: "service_images");

            migrationBuilder.DropPrimaryKey(
                name: "PK_service_images",
                table: "service_images");

            migrationBuilder.RenameTable(
                name: "service_images",
                newName: "ServiceImages");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "ServiceImages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "service_id",
                table: "ServiceImages",
                newName: "ServiceId");

            migrationBuilder.RenameColumn(
                name: "image_path",
                table: "ServiceImages",
                newName: "ImagePath");

            migrationBuilder.RenameColumn(
                name: "created_on",
                table: "ServiceImages",
                newName: "CreatedOn");

            migrationBuilder.RenameIndex(
                name: "IX_service_images_service_id",
                table: "ServiceImages",
                newName: "IX_ServiceImages_ServiceId");

            migrationBuilder.AlterColumn<string>(
                name: "ImagePath",
                table: "ServiceImages",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceImages",
                table: "ServiceImages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceImages_services_ServiceId",
                table: "ServiceImages",
                column: "ServiceId",
                principalTable: "services",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
