using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTablesNaming : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_ServiceTypes_ServiceTypeId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_services_SubCategories_sub_category_id",
                table: "services");

            migrationBuilder.DropForeignKey(
                name: "FK_SubCategories_Categories_CategoryId",
                table: "SubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SubCategories",
                table: "SubCategories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServiceTypes",
                table: "ServiceTypes");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "categories");

            migrationBuilder.RenameTable(
                name: "SubCategories",
                newName: "sub_categories");

            migrationBuilder.RenameTable(
                name: "ServiceTypes",
                newName: "service_types");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_ServiceTypeId_Name",
                table: "categories",
                newName: "IX_categories_ServiceTypeId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_SubCategories_CategoryId_Name",
                table: "sub_categories",
                newName: "IX_sub_categories_CategoryId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_ServiceTypes_Name",
                table: "service_types",
                newName: "IX_service_types_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_categories",
                table: "categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_sub_categories",
                table: "sub_categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_service_types",
                table: "service_types",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_categories_service_types_ServiceTypeId",
                table: "categories",
                column: "ServiceTypeId",
                principalTable: "service_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_services_sub_categories_sub_category_id",
                table: "services",
                column: "sub_category_id",
                principalTable: "sub_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sub_categories_categories_CategoryId",
                table: "sub_categories",
                column: "CategoryId",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_categories_service_types_ServiceTypeId",
                table: "categories");

            migrationBuilder.DropForeignKey(
                name: "FK_services_sub_categories_sub_category_id",
                table: "services");

            migrationBuilder.DropForeignKey(
                name: "FK_sub_categories_categories_CategoryId",
                table: "sub_categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_categories",
                table: "categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_sub_categories",
                table: "sub_categories");

            migrationBuilder.DropPrimaryKey(
                name: "PK_service_types",
                table: "service_types");

            migrationBuilder.RenameTable(
                name: "categories",
                newName: "Categories");

            migrationBuilder.RenameTable(
                name: "sub_categories",
                newName: "SubCategories");

            migrationBuilder.RenameTable(
                name: "service_types",
                newName: "ServiceTypes");

            migrationBuilder.RenameIndex(
                name: "IX_categories_ServiceTypeId_Name",
                table: "Categories",
                newName: "IX_Categories_ServiceTypeId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_sub_categories_CategoryId_Name",
                table: "SubCategories",
                newName: "IX_SubCategories_CategoryId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_service_types_Name",
                table: "ServiceTypes",
                newName: "IX_ServiceTypes_Name");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SubCategories",
                table: "SubCategories",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServiceTypes",
                table: "ServiceTypes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_ServiceTypes_ServiceTypeId",
                table: "Categories",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_services_SubCategories_sub_category_id",
                table: "services",
                column: "sub_category_id",
                principalTable: "SubCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SubCategories_Categories_CategoryId",
                table: "SubCategories",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
