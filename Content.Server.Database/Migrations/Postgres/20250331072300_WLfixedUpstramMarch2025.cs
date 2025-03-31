using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class WLfixedUpstramMarch2025 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ooc_text",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "voice",
                table: "profile",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "deadminned",
                table: "admin",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "suspended",
                table: "admin",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "discord_connections",
                columns: table => new
                {
                    discord_connections_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    discord_id = table.Column<string>(type: "text", nullable: false),
                    user_guid = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discord_connections", x => x.discord_connections_id);
                });

            migrationBuilder.CreateTable(
                name: "ipintel_cache",
                columns: table => new
                {
                    ipintel_cache_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    address = table.Column<IPAddress>(type: "inet", nullable: false),
                    time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    score = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ipintel_cache", x => x.ipintel_cache_id);
                });

            migrationBuilder.CreateTable(
                name: "job_subname",
                columns: table => new
                {
                    job_subname_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    profile_id = table.Column<int>(type: "integer", nullable: false),
                    job_name = table.Column<string>(type: "text", nullable: false),
                    subname = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_subname", x => x.job_subname_id);
                    table.ForeignKey(
                        name: "FK_job_subname_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "job_unblocking",
                columns: table => new
                {
                    job_unblocking_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    profile_id = table.Column<int>(type: "integer", nullable: false),
                    job_name = table.Column<string>(type: "text", nullable: false),
                    force_unblocked = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_job_unblocking", x => x.job_unblocking_id);
                    table.ForeignKey(
                        name: "FK_job_unblocking_profile_profile_id",
                        column: x => x.profile_id,
                        principalTable: "profile",
                        principalColumn: "profile_id",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateIndex(
                name: "IX_job_subname_profile_id_job_name",
                table: "job_subname",
                columns: new[] { "profile_id", "job_name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_job_unblocking_profile_id_job_name",
                table: "job_unblocking",
                columns: new[] { "profile_id", "job_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discord_connections");

            migrationBuilder.DropTable(
                name: "ipintel_cache");

            migrationBuilder.DropTable(
                name: "job_subname");

            migrationBuilder.DropTable(
                name: "job_unblocking");

            migrationBuilder.DropColumn(
                name: "height",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "ooc_text",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "voice",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "deadminned",
                table: "admin");

            migrationBuilder.DropColumn(
                name: "suspended",
                table: "admin");
        }
    }
}
