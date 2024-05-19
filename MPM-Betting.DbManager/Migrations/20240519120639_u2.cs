using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class u2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementMpmUser_Achievements_AchievmentsId",
                table: "AchievementMpmUser");

            migrationBuilder.RenameColumn(
                name: "AchievmentsId",
                table: "AchievementMpmUser",
                newName: "AchievementsId");

            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                table: "AspNetUsers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementMpmUser_Achievements_AchievementsId",
                table: "AchievementMpmUser",
                column: "AchievementsId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AchievementMpmUser_Achievements_AchievementsId",
                table: "AchievementMpmUser");

            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "AchievementsId",
                table: "AchievementMpmUser",
                newName: "AchievmentsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AchievementMpmUser_Achievements_AchievmentsId",
                table: "AchievementMpmUser",
                column: "AchievmentsId",
                principalTable: "Achievements",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
