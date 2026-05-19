using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPartnerOtpTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PartnerOtpVerifications_service_partners_ServicePartnerId",
                table: "PartnerOtpVerifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PartnerOtpVerifications",
                table: "PartnerOtpVerifications");

            migrationBuilder.RenameTable(
                name: "PartnerOtpVerifications",
                newName: "partner_otp_verifications");

            migrationBuilder.RenameIndex(
                name: "IX_PartnerOtpVerifications_ServicePartnerId",
                table: "partner_otp_verifications",
                newName: "IX_partner_otp_verifications_ServicePartnerId");

            migrationBuilder.AlterColumn<string>(
                name: "OtpCode",
                table: "partner_otp_verifications",
                type: "character(4)",
                fixedLength: true,
                maxLength: 4,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "partner_otp_verifications",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRevoked",
                table: "partner_otp_verifications",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "partner_otp_verifications",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_partner_otp_verifications",
                table: "partner_otp_verifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_partner_otp_verifications_service_partners_ServicePartnerId",
                table: "partner_otp_verifications",
                column: "ServicePartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_partner_otp_verifications_service_partners_ServicePartnerId",
                table: "partner_otp_verifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_partner_otp_verifications",
                table: "partner_otp_verifications");

            migrationBuilder.RenameTable(
                name: "partner_otp_verifications",
                newName: "PartnerOtpVerifications");

            migrationBuilder.RenameIndex(
                name: "IX_partner_otp_verifications_ServicePartnerId",
                table: "PartnerOtpVerifications",
                newName: "IX_PartnerOtpVerifications_ServicePartnerId");

            migrationBuilder.AlterColumn<string>(
                name: "OtpCode",
                table: "PartnerOtpVerifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character(4)",
                oldFixedLength: true,
                oldMaxLength: 4);

            migrationBuilder.AlterColumn<bool>(
                name: "IsUsed",
                table: "PartnerOtpVerifications",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRevoked",
                table: "PartnerOtpVerifications",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "PartnerOtpVerifications",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddPrimaryKey(
                name: "PK_PartnerOtpVerifications",
                table: "PartnerOtpVerifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PartnerOtpVerifications_service_partners_ServicePartnerId",
                table: "PartnerOtpVerifications",
                column: "ServicePartnerId",
                principalTable: "service_partners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
