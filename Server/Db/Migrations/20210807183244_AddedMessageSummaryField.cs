using Microsoft.EntityFrameworkCore.Migrations;

namespace Server.Db.Migrations
{
    public partial class AddedMessageSummaryField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MessageSummary",
                table: "Bookmarks",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MessageSummary",
                table: "Bookmarks");
        }
    }
}
