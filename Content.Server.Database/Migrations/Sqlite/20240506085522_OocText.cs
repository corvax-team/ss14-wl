using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class OocText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropColumn(
            //    name: "backpack",
            //    table: "profile");

            //migrationBuilder.RenameColumn(
            //    name: "clothing",
            //    table: "profile",
            //    newName: "ooc_text");

            migrationBuilder.DropColumn(
                name: "height",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "ooc_text",
                table: "profile");

            migrationBuilder.AddColumn<int>(
                name: "height",
                table: "profile",
                type: "INTEGER",
                nullable: false,
                defaultValue: 140);

            migrationBuilder.AddColumn<string>(
                name: "ooc_text",
                table: "profile",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            //migrationBuilder.CreateTable(
            //    name: "profile_role_loadout",
            //    columns: table => new
            //    {
            //        profile_role_loadout_id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        profile_id = table.Column<int>(type: "INTEGER", nullable: false),
            //        role_name = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_profile_role_loadout", x => x.profile_role_loadout_id);
            //        table.ForeignKey(
            //            name: "FK_profile_role_loadout_profile_profile_id",
            //            column: x => x.profile_id,
            //            principalTable: "profile",
            //            principalColumn: "profile_id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "profile_loadout_group",
            //    columns: table => new
            //    {
            //        profile_loadout_group_id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        profile_role_loadout_id = table.Column<int>(type: "INTEGER", nullable: false),
            //        group_name = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_profile_loadout_group", x => x.profile_loadout_group_id);
            //        table.ForeignKey(
            //            name: "FK_profile_loadout_group_profile_role_loadout_profile_role_loadout_id",
            //            column: x => x.profile_role_loadout_id,
            //            principalTable: "profile_role_loadout",
            //            principalColumn: "profile_role_loadout_id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "profile_loadout",
            //    columns: table => new
            //    {
            //        profile_loadout_id = table.Column<int>(type: "INTEGER", nullable: false)
            //            .Annotation("Sqlite:Autoincrement", true),
            //        profile_loadout_group_id = table.Column<int>(type: "INTEGER", nullable: false),
            //        loadout_name = table.Column<string>(type: "TEXT", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_profile_loadout", x => x.profile_loadout_id);
            //        table.ForeignKey(
            //            name: "FK_profile_loadout_profile_loadout_group_profile_loadout_group_id",
            //            column: x => x.profile_loadout_group_id,
            //            principalTable: "profile_loadout_group",
            //            principalColumn: "profile_loadout_group_id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_profile_loadout_profile_loadout_group_id",
            //    table: "profile_loadout",
            //    column: "profile_loadout_group_id");

            //migrationBuilder.CreateIndex(
            //    name: "IX_profile_loadout_group_profile_role_loadout_id",
            //    table: "profile_loadout_group",
            //    column: "profile_role_loadout_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropTable(
            //    name: "profile_loadout");

            //migrationBuilder.DropTable(
            //    name: "profile_loadout_group");

            //migrationBuilder.DropTable(
            //    name: "profile_role_loadout
            migrationBuilder.DropColumn(
                name: "ooc_text",
                table: "profile");

            migrationBuilder.DropColumn(
                name: "height",
                table: "profile");

            //migrationBuilder.RenameColumn(
            //    name: "ooc_text",
            //    table: "profile",
            //    newName: "clothing");

            //migrationBuilder.AddColumn<string>(
            //    name: "backpack",
            //    table: "profile",
            //    type: "TEXT",
            //    nullable: false,
            //    defaultValue: "");
        }
    }
}
