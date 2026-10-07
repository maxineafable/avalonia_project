using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AvaloniaProject.Migrations
{
    /// <inheritdoc />
    public partial class AddTypeAndSteamAppId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SteamAppId",
                table: "Games",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Games",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SteamAppId",
                table: "Games");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Games");
        }
    }
}
