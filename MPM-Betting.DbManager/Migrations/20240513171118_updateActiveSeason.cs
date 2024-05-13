using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class updateActiveSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_SeasonEntries_CurrentSeasonId",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CurrentSeasonId",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CurrentSeasonId",
                table: "Groups");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SeasonEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SeasonEntries");

            migrationBuilder.AddColumn<int>(
                name: "CurrentSeasonId",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CurrentSeasonId",
                table: "Groups",
                column: "CurrentSeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_SeasonEntries_CurrentSeasonId",
                table: "Groups",
                column: "CurrentSeasonId",
                principalTable: "SeasonEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
