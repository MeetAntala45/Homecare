using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCofiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerLeaves_service_partners_PartnerId",
                table: "PartnerLeaves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerLeaves",
                table: "PartnerLeaves");

            migrationBuilder.RenameTable(
                name: "PartnerLeaves",
                newName: "partner_leaves");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerLeaves_PartnerId",
                table: "partner_leaves",
                newName: "IX_partner_leaves_PartnerId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "partner_leaves",
                type: "integer",
                nullable: false,
                defaultValue: 1,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "partner_leaves",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "AdminRemarks",
                table: "partner_leaves",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServicePartnerId",
                table: "partner_leaves",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_partner_leaves",
                table: "partner_leaves",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_partner_leaves_ServicePartnerId",
                table: "partner_leaves",
                column: "ServicePartnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_partner_leaves_service_partners_PartnerId",
                table: "partner_leaves",
                column: "PartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_partner_leaves_service_partners_ServicePartnerId",
                table: "partner_leaves",
                column: "ServicePartnerId",
                principalTable: "service_partners",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_partner_leaves_service_partners_PartnerId",
                table: "partner_leaves");

            migrationBuilder.DropForeignKey(
                name: "FK_partner_leaves_service_partners_ServicePartnerId",
                table: "partner_leaves");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partner_leaves",
                table: "partner_leaves");

            migrationBuilder.DropIndex(
                name: "IX_partner_leaves_ServicePartnerId",
                table: "partner_leaves");

            migrationBuilder.DropColumn(
                name: "ServicePartnerId",
                table: "partner_leaves");

            migrationBuilder.RenameTable(
                name: "partner_leaves",
                newName: "PartnerLeaves");

            migrationBuilder.RenameIndex(
                name: "IX_partner_leaves_PartnerId",
                table: "PartnerLeaves",
                newName: "IX_PartnerLeaves_PartnerId");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "PartnerLeaves",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "PartnerLeaves",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "AdminRemarks",
                table: "PartnerLeaves",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerLeaves",
                table: "PartnerLeaves",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerLeaves_service_partners_PartnerId",
                table: "PartnerLeaves",
                column: "PartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
