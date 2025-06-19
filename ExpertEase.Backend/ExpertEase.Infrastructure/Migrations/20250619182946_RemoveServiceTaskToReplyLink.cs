using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveServiceTaskToReplyLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTask_Reply_ReplyId",
                table: "ServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTask_ReplyId",
                table: "ServiceTask");

            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "ServiceTask");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ReplyId",
                table: "ServiceTask",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTask_ReplyId",
                table: "ServiceTask",
                column: "ReplyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTask_Reply_ReplyId",
                table: "ServiceTask",
                column: "ReplyId",
                principalTable: "Reply",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
