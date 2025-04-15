using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_User_SenderUserId",
                table: "Request");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_InitiatorUserId",
                table: "Transaction");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_User_SenderUserId",
                table: "Request",
                column: "SenderUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_InitiatorUserId",
                table: "Transaction",
                column: "InitiatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_User_SenderUserId",
                table: "Request");

            migrationBuilder.DropForeignKey(
                name: "FK_Transaction_User_InitiatorUserId",
                table: "Transaction");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_User_SenderUserId",
                table: "Request",
                column: "SenderUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transaction_User_InitiatorUserId",
                table: "Transaction",
                column: "InitiatorUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
