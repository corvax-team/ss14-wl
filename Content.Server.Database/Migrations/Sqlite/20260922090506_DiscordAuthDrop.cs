using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class DiscordAuthDrop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_connections");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "discord_connections",
                columns: table => new
                {
                    discord_connections_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    discord_id = table.Column<string>(type: "TEXT", nullable: false),
                    user_guid = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_connections", x => x.discord_connections_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discord_connections_discord_id",
                table: "discord_connections",
                column: "discord_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_discord_connections_user_guid",
                table: "discord_connections",
                column: "user_guid",
                unique: true);
        }
    }
}
