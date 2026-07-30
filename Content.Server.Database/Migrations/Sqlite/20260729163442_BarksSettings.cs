using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class BarksSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "bark_max_delay",
                table: "profile",
                type: "REAL",
                nullable: false,
                defaultValue: 0.5f);

            migrationBuilder.AddColumn<float>(
                name: "bark_min_delay",
                table: "profile",
                type: "REAL",
                nullable: false,
                defaultValue: 0.1f);

            migrationBuilder.AddColumn<float>(
                name: "bark_pitch",
                table: "profile",
                type: "REAL",
                nullable: false,
                defaultValue: 1f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "bark_max_delay",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "bark_min_delay",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "bark_pitch",
                table: "profile");
        }
    }
}
