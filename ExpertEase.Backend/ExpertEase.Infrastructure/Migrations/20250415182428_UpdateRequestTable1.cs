using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRequestTable1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "RequestDate",
                table: "Request",
                newName: "RequestedStartDate");

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                table: "Request",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectedAt",
                table: "Request");

            migrationBuilder.RenameColumn(
                name: "RequestedStartDate",
                table: "Request",
                newName: "RequestDate");
        }
    }
}
