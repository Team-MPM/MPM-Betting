using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class refactor1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavoriteSeasons");

            migrationBuilder.DropTable(
                name: "FootballScoreBets");

            migrationBuilder.DropColumn(
                name: "QuoteAway",
                table: "FootballResultBets");

            migrationBuilder.DropColumn(
                name: "QuoteDraw",
                table: "FootballResultBets");

            migrationBuilder.RenameColumn(
                name: "LeaueeId",
                table: "UserFavouriteSeasons",
                newName: "LeagueId");

            migrationBuilder.RenameColumn(
                name: "Result",
                table: "FootballResultBets",
                newName: "HomeScore");

            migrationBuilder.RenameColumn(
                name: "QuoteHome",
                table: "FootballResultBets",
                newName: "AwayScore");

            migrationBuilder.RenameColumn(
                name: "Score",
                table: "Bets",
                newName: "Points");

            migrationBuilder.AddColumn<double>(
                name: "Quote",
                table: "FootballResultBets",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Quote",
                table: "FootballResultBets");

            migrationBuilder.RenameColumn(
                name: "LeagueId",
                table: "UserFavouriteSeasons",
                newName: "LeaueeId");

            migrationBuilder.RenameColumn(
                name: "HomeScore",
                table: "FootballResultBets",
                newName: "Result");

            migrationBuilder.RenameColumn(
                name: "AwayScore",
                table: "FootballResultBets",
                newName: "QuoteHome");

            migrationBuilder.RenameColumn(
                name: "Points",
                table: "Bets",
                newName: "Score");

            migrationBuilder.AddColumn<int>(
                name: "QuoteAway",
                table: "FootballResultBets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QuoteDraw",
                table: "FootballResultBets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "FavoriteSeasons",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SeasonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavoriteSeasons", x => new { x.UserId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_FavoriteSeasons_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FavoriteSeasons_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballScoreBets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    AwayScore = table.Column<int>(type: "int", nullable: false),
                    HomeScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballScoreBets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballScoreBets_Bets_Id",
                        column: x => x.Id,
                        principalTable: "Bets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavoriteSeasons_SeasonId",
                table: "FavoriteSeasons",
                column: "SeasonId");
        }
    }
}
