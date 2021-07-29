using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Server.Db.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "Bookmark",
                columns: table => new
                {
                    BookmarkId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AuthorSnowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ChannelSnowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    MessageSnowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserSnowflake = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    UserSnowflake1 = table.Column<decimal>(type: "numeric(20,0)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookmark", x => x.BookmarkId);
                    table.ForeignKey(
                        name: "FK_Bookmark_Users_UserSnowflake1",
                        column: x => x.UserSnowflake1,
                        principalTable: "Users",
                        principalColumn: "UserSnowflake",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_MessageSnowflake_UserSnowflake",
                table: "Bookmark",
                columns: new[] { "MessageSnowflake", "UserSnowflake" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bookmark_UserSnowflake1",
                table: "Bookmark",
                column: "UserSnowflake1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bookmark");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
