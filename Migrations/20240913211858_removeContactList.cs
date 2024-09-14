using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Chat_App.Migrations
{
    /// <inheritdoc />
    public partial class removeContactList : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ContactLists_ContactsId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ContactsId",
                table: "AspNetUsers",
                newName: "ContactListId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_ContactsId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_ContactListId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_UserId",
                table: "AspNetUsers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ContactLists_ContactListId",
                table: "AspNetUsers",
                column: "ContactListId",
                principalTable: "ContactLists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetUsers_UserId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_ContactLists_ContactListId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "ContactListId",
                table: "AspNetUsers",
                newName: "ContactsId");

            migrationBuilder.RenameIndex(
                name: "IX_AspNetUsers_ContactListId",
                table: "AspNetUsers",
                newName: "IX_AspNetUsers_ContactsId");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_ContactLists_ContactsId",
                table: "AspNetUsers",
                column: "ContactsId",
                principalTable: "ContactLists",
                principalColumn: "Id");
        }
    }
}
