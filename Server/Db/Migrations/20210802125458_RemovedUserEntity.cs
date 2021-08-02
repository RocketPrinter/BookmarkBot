using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Db.Migrations
{
    public partial class RemovedUserEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookmark_Users_UserSnowflake1",
                table: "Bookmark");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookmark",
                table: "Bookmark");

            migrationBuilder.DropIndex(
                name: "IX_Bookmark_UserSnowflake1",
                table: "Bookmark");

            migrationBuilder.RenameTable(
                name: "Bookmark",
                newName: "Bookmarks");

            migrationBuilder.RenameColumn(
                name: "UserSnowflake1",
                table: "Bookmarks",
                newName: "GuildSnowFlake");

            migrationBuilder.RenameIndex(
                name: "IX_Bookmark_MessageSnowflake_UserSnowflake",
                table: "Bookmarks",
                newName: "IX_Bookmarks_MessageSnowflake_UserSnowflake");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookmarks",
                table: "Bookmarks",
                column: "BookmarkId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Bookmarks",
                table: "Bookmarks");

            migrationBuilder.RenameTable(
                name: "Bookmarks",
                newName: "Bookmark");

            migrationBuilder.RenameColumn(
                name: "GuildSnowFlake",
                table: "Bookmark",
                newName: "UserSnowflake1");

            migrationBuilder.RenameIndex(
                name: "IX_Bookmarks_MessageSnowflake_UserSnowflake",
                table: "Bookmark",
                newName: "IX_Bookmark_MessageSnowflake_UserSnowflake");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Bookmark",
                table: "Bookmark",
                column: "BookmarkId");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserSnowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserSnowflake);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_UserSnowflake1",
                table: "Bookmark",
                column: "UserSnowflake1");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookmark_Users_UserSnowflake1",
                table: "Bookmark",
                column: "UserSnowflake1",
                principalTable: "Users",
                principalColumn: "UserSnowflake",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
