using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class achievements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievmentMpmUser");

            migrationBuilder.DropTable(
                name: "Achievment");

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AchievementEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AchievementId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DateEarned = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AchievementEntries_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievementEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AchievementMpmUser",
                columns: table => new
                {
                    AchievmentsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievementMpmUser", x => new { x.AchievmentsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AchievementMpmUser_Achievements_AchievmentsId",
                        column: x => x.AchievmentsId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AchievementMpmUser_AspNetUsers_UsersId",
                        column: x => x.UsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AchievementEntries_AchievementId",
                table: "AchievementEntries",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementEntries_UserId",
                table: "AchievementEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AchievementMpmUser_UsersId",
                table: "AchievementMpmUser",
                column: "UsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AchievementEntries");

            migrationBuilder.DropTable(
                name: "AchievementMpmUser");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.CreateTable(
                name: "Achievment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AchievmentMpmUser",
                columns: table => new
                {
                    AchievmentsId = table.Column<int>(type: "int", nullable: false),
                    UsersId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AchievmentMpmUser", x => new { x.AchievmentsId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_AchievmentMpmUser_Achievment_AchievmentsId",
                        column: x => x.AchievmentsId,
                        principalTable: "Achievment",
                        principalColumn: "Id",
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
    }
}
