using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSupportName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Supports",
                table: "Supports");

            migrationBuilder.RenameTable(
                name: "Supports",
                newName: "supports");

            migrationBuilder.RenameIndex(
                name: "IX_Supports_Mobile",
                table: "supports",
                newName: "IX_supports_Mobile");

            migrationBuilder.RenameIndex(
                name: "IX_Supports_Email",
                table: "supports",
                newName: "IX_supports_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_supports",
                table: "supports",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_supports",
                table: "supports");

            migrationBuilder.RenameTable(
                name: "supports",
                newName: "Supports");

            migrationBuilder.RenameIndex(
                name: "IX_supports_Mobile",
                table: "Supports",
                newName: "IX_Supports_Mobile");

            migrationBuilder.RenameIndex(
                name: "IX_supports_Email",
                table: "Supports",
                newName: "IX_Supports_Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supports",
                table: "Supports",
                column: "Id");
        }
    }
}
