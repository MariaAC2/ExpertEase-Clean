using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_User_ReceiverUserId",
                table: "Request");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_User_ReceiverUserId",
                table: "Request",
                column: "ReceiverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_User_ReceiverUserId",
                table: "Request");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_User_ReceiverUserId",
                table: "Request",
                column: "ReceiverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
