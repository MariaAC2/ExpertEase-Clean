using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedSpecialistProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Photo");

            migrationBuilder.AddColumn<List<string>>(
                name: "Portfolio",
                table: "SpecialistProfile",
                type: "text[]",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Portfolio",
                table: "SpecialistProfile");

            migrationBuilder.CreateTable(
                name: "Photo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    IsProfilePicture = table.Column<bool>(type: "boolean", nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    SpecialistProfileUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Url = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
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
                name: "IX_Photo_SpecialistProfileUserId",
                table: "Photo",
                column: "SpecialistProfileUserId");
        }
    }
}
