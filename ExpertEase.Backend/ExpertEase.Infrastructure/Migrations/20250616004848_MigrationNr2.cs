using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrationNr2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portfolio",
                table: "SpecialistProfile");

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsProfilePicture = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SpecialistProfileUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Photo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Photo_SpecialistProfile_SpecialistProfileUserId",
                        column: x => x.SpecialistProfileUserId,
                        principalTable: "SpecialistProfile",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTask_SpecialistId",
                table: "ServiceTask",
                column: "SpecialistId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTask_UserId",
                table: "ServiceTask",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Photo_SpecialistProfileUserId",
                table: "Photo",
                column: "SpecialistProfileUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTask_User_SpecialistId",
                table: "ServiceTask",
                column: "SpecialistId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTask_User_UserId",
                table: "ServiceTask",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTask_User_SpecialistId",
                table: "ServiceTask");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTask_User_UserId",
                table: "ServiceTask");

            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTask_SpecialistId",
                table: "ServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTask_UserId",
                table: "ServiceTask");

            migrationBuilder.AddColumn<List<string>>(
                name: "Portfolio",
                table: "SpecialistProfile",
                type: "text[]",
                nullable: false);
        }
    }
}
