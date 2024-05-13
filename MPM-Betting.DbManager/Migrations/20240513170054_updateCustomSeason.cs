using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class updateCustomSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomSeasonEntries_CustomSeasons_SeasonId",
                table: "CustomSeasonEntries");

            migrationBuilder.DropTable(
                name: "CustomSeasons");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Seasons",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_CustomSeasonEntries_Seasons_SeasonId",
                table: "CustomSeasonEntries",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CustomSeasonEntries_Seasons_SeasonId",
                table: "CustomSeasonEntries");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Seasons");

            migrationBuilder.CreateTable(
                name: "CustomSeasons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomSeasons", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_CustomSeasonEntries_CustomSeasons_SeasonId",
                table: "CustomSeasonEntries",
                column: "SeasonId",
                principalTable: "CustomSeasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
