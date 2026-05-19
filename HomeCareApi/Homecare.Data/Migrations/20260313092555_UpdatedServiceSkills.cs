using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedServiceSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_partner_services_offered_sub_categories_SubCategoryId1",
                table: "partner_services_offered");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_skills_categories_CategoryId1",
                table: "partner_skills");

            migrationBuilder.DropIndex(
                name: "IX_partner_skills_CategoryId1",
                table: "partner_skills");

            migrationBuilder.DropIndex(
                name: "IX_partner_services_offered_SubCategoryId1",
                table: "partner_services_offered");

            migrationBuilder.DropColumn(
                name: "CategoryId1",
                table: "partner_skills");

            migrationBuilder.DropColumn(
                name: "SubCategoryId1",
                table: "partner_services_offered");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId1",
                table: "partner_skills",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SubCategoryId1",
                table: "partner_services_offered",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_partner_skills_CategoryId1",
                table: "partner_skills",
                column: "CategoryId1");

            migrationBuilder.CreateIndex(
                name: "IX_partner_services_offered_SubCategoryId1",
                table: "partner_services_offered",
                column: "SubCategoryId1");

            migrationBuilder.AddForeignKey(
                name: "FK_partner_services_offered_sub_categories_SubCategoryId1",
                table: "partner_services_offered",
                column: "SubCategoryId1",
                principalTable: "sub_categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_skills_categories_CategoryId1",
                table: "partner_skills",
                column: "CategoryId1",
                principalTable: "categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
