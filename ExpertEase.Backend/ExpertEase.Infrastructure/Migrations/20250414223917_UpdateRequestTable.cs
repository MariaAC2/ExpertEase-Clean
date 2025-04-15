using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Specialist_ReceiverSpecialistId",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "ReceiverSpecialistId",
                table: "Request",
                newName: "ReceiverUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Request_ReceiverSpecialistId",
                table: "Request",
                newName: "IX_Request_ReceiverUserId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Request",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Request",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_User_ReceiverUserId",
                table: "Request",
                column: "ReceiverUserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_User_ReceiverUserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "Address",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "ReceiverUserId",
                table: "Request",
                newName: "ReceiverSpecialistId");

            migrationBuilder.RenameIndex(
                name: "IX_Request_ReceiverUserId",
                table: "Request",
                newName: "IX_Request_ReceiverSpecialistId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Specialist_ReceiverSpecialistId",
                table: "Request",
                column: "ReceiverSpecialistId",
                principalTable: "Specialist",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
