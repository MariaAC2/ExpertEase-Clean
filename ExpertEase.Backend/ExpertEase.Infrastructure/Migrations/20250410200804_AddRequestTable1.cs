using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Request_Specialist_SpecialistUserId",
                table: "Request");

            migrationBuilder.DropForeignKey(
                name: "FK_Request_User_UserId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_SpecialistUserId",
                table: "Request");

            migrationBuilder.DropIndex(
                name: "IX_Request_UserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "SpecialistUserId",
                table: "Request");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Request");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SpecialistUserId",
                table: "Request",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Request",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Request_SpecialistUserId",
                table: "Request",
                column: "SpecialistUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Request_UserId",
                table: "Request",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_Specialist_SpecialistUserId",
                table: "Request",
                column: "SpecialistUserId",
                principalTable: "Specialist",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Request_User_UserId",
                table: "Request",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
