using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class VulpkaninToCorvaxVulpkanin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "profile",
                keyColumn: "species",
                keyValue: "Vulpkanin",
                column: "species",
                value: "CorvaxVulpkanin");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "profile",
                keyColumn: "species",
                keyValue: "CorvaxVulpkanin",
                column: "species",
                value: "Vulpkanin");
        }
    }
}
