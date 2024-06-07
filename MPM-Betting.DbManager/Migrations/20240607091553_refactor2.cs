using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class refactor2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Games_GameId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballResultBets_Bets_Id",
                table: "FootballResultBets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FootballResultBets",
                table: "FootballResultBets");

            migrationBuilder.RenameTable(
                name: "FootballResultBets",
                newName: "FootballGameBets");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "Bets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "ResultHit",
                table: "FootballGameBets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ScoreHit",
                table: "FootballGameBets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FootballGameBets",
                table: "FootballGameBets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Games_GameId",
                table: "Bets",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FootballGameBets_Bets_Id",
                table: "FootballGameBets",
                column: "Id",
                principalTable: "Bets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Games_GameId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_FootballGameBets_Bets_Id",
                table: "FootballGameBets");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FootballGameBets",
                table: "FootballGameBets");

            migrationBuilder.DropColumn(
                name: "ResultHit",
                table: "FootballGameBets");

            migrationBuilder.DropColumn(
                name: "ScoreHit",
                table: "FootballGameBets");

            migrationBuilder.RenameTable(
                name: "FootballGameBets",
                newName: "FootballResultBets");

            migrationBuilder.AlterColumn<int>(
                name: "GameId",
                table: "Bets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_FootballResultBets",
                table: "FootballResultBets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Games_GameId",
                table: "Bets",
                column: "GameId",
                principalTable: "Games",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FootballResultBets_Bets_Id",
                table: "FootballResultBets",
                column: "Id",
                principalTable: "Bets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
