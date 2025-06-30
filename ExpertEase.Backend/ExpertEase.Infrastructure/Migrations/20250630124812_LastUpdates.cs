using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LastUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ServiceTask",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferReference",
                table: "ServiceTask",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferredAt",
                table: "ServiceTask",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRefunded",
                table: "Payment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTransferred",
                table: "Payment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RefundReference",
                table: "Payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TransferReference",
                table: "Payment",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TransferredAt",
                table: "Payment",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ServiceTask");

            migrationBuilder.DropColumn(
                name: "TransferReference",
                table: "ServiceTask");

            migrationBuilder.DropColumn(
                name: "TransferredAt",
                table: "ServiceTask");

            migrationBuilder.DropColumn(
                name: "IsRefunded",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "IsTransferred",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "RefundReference",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "TransferReference",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "TransferredAt",
                table: "Payment");
        }
    }
}
