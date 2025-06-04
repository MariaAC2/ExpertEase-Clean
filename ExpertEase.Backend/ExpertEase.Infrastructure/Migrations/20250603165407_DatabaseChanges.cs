using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ServiceTaskId",
                table: "Review",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Review_ServiceTaskId",
                table: "Review",
                column: "ServiceTaskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_ServiceTask_ServiceTaskId",
                table: "Review",
                column: "ServiceTaskId",
                principalTable: "ServiceTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_ServiceTask_ServiceTaskId",
                table: "Review");

            migrationBuilder.DropIndex(
                name: "IX_Review_ServiceTaskId",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ServiceTaskId",
                table: "Review");
        }
    }
}
