using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MPM_Betting.DbManager.Migrations
{
    /// <inheritdoc />
    public partial class upd2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavouriteSeasons",
                table: "UserFavouriteSeasons");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavouriteSeasons",
                table: "UserFavouriteSeasons",
                columns: new[] { "UserId", "LeaueeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_UserFavouriteSeasons",
                table: "UserFavouriteSeasons");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserFavouriteSeasons",
                table: "UserFavouriteSeasons",
                column: "UserId");
        }
    }
}
