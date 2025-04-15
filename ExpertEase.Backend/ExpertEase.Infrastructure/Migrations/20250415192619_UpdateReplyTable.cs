using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReplyTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reply_Request_RequestId",
                table: "Reply");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reply",
                table: "Reply");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reply",
                table: "Reply",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_RequestId",
                table: "Reply",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reply_Request_RequestId",
                table: "Reply",
                column: "RequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reply_Request_RequestId",
                table: "Reply");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Reply",
                table: "Reply");

            migrationBuilder.DropIndex(
                name: "IX_Reply_RequestId",
                table: "Reply");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Reply",
                table: "Reply",
                column: "RequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reply_Request_RequestId",
                table: "Reply",
                column: "RequestId",
                principalTable: "Request",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
