using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_User_ReceiverUserId",
                table: "Review");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_User_ReceiverUserId",
                table: "Review",
                column: "ReceiverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_User_ReceiverUserId",
                table: "Review");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_User_ReceiverUserId",
                table: "Review",
                column: "ReceiverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
