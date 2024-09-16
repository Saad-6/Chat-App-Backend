using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat_App.Migrations
{
    /// <inheritdoc />
    public partial class improvingFriendRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SenderName",
                table: "FriendRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SenderPicture",
                table: "FriendRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenderName",
                table: "FriendRequests");

            migrationBuilder.DropColumn(
                name: "SenderPicture",
                table: "FriendRequests");
        }
    }
}
