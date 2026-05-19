using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class PartnerSkillConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerSkills_service_partners_PartnerId",
                table: "PartnerSkills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerSkills",
                table: "PartnerSkills");

            migrationBuilder.RenameTable(
                name: "PartnerSkills",
                newName: "partner_skills");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerSkills_PartnerId",
                table: "partner_skills",
                newName: "IX_partner_skills_PartnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_partner_skills",
                table: "partner_skills",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_partner_skills_service_partners_PartnerId",
                table: "partner_skills",
                column: "PartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_partner_skills_service_partners_PartnerId",
                table: "partner_skills");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partner_skills",
                table: "partner_skills");

            migrationBuilder.RenameTable(
                name: "partner_skills",
                newName: "PartnerSkills");

            migrationBuilder.RenameIndex(
                name: "IX_partner_skills_PartnerId",
                table: "PartnerSkills",
                newName: "IX_PartnerSkills_PartnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerSkills",
                table: "PartnerSkills",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerSkills_service_partners_PartnerId",
                table: "PartnerSkills",
                column: "PartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
