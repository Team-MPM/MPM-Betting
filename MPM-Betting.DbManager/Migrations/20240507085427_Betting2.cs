using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class Betting2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupLimit",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ActualResult",
                table: "Bets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuessedAwayScore",
                table: "Bets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuessedHomeScore",
                table: "Bets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GuessedResult",
                table: "Bets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Achievment",
                columns: table => new
                {
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievment", x => x.Title);
                });

            migrationBuilder.CreateTable(
                name: "AchievmentMpmUser",
                columns: table => new
                {
                    AchievmentsTitle = table.Column<string>(type: "nvarchar(50)", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievmentMpmUser", x => new { x.AchievmentsTitle, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AchievmentMpmUser_Achievment_AchievmentsTitle",
                        column: x => x.AchievmentsTitle,
                        principalTable: "Achievment",
                        principalColumn: "Title",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievmentMpmUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievmentMpmUser_UsersId",
                table: "AchievmentMpmUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievmentMpmUser");

            migrationBuilder.DropTable(
                name: "Achievment");

            migrationBuilder.DropColumn(
                name: "GroupLimit",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ActualResult",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "GuessedAwayScore",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "GuessedHomeScore",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "GuessedResult",
                table: "Bets");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "AspNetUsers");
        }
    }
}
