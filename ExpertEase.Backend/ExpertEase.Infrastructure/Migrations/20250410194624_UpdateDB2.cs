using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDB2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialist",
                table: "Specialist");

            migrationBuilder.DropIndex(
                name: "IX_Specialist_UserId",
                table: "Specialist");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Specialist");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Specialist");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Specialist");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialist",
                table: "Specialist",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialist",
                table: "Specialist");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Specialist",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Specialist",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Specialist",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialist",
                table: "Specialist",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Specialist_UserId",
                table: "Specialist",
                column: "UserId",
                unique: true);
        }
    }
}
