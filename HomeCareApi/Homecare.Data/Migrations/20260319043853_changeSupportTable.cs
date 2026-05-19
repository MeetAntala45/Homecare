using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Homecare.Data.Migrations
{
    /// <inheritdoc />
    public partial class changeSupportTable : Migration
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

            migrationBuilder.AddPrimaryKey(
                name: "PK_Supports",
                table: "Supports",
                column: "Id");
        }
    }
}
