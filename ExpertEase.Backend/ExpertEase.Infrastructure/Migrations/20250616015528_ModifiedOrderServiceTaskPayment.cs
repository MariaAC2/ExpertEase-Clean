using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedOrderServiceTaskPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_ServiceTask_ServiceTaskId",
                table: "Payment");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTask_ReplyId",
                table: "ServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_Payment_ServiceTaskId",
                table: "Payment");

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentId",
                table: "ServiceTask",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceTaskId",
                table: "Payment",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ReplyId",
                table: "Payment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTask_PaymentId",
                table: "ServiceTask",
                column: "PaymentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTask_ReplyId",
                table: "ServiceTask",
                column: "ReplyId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ReplyId",
                table: "Payment",
                column: "ReplyId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_Reply_ReplyId",
                table: "Payment",
                column: "ReplyId",
                principalTable: "Reply",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTask_Payment_PaymentId",
                table: "ServiceTask",
                column: "PaymentId",
                principalTable: "Payment",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Payment_Reply_ReplyId",
                table: "Payment");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTask_Payment_PaymentId",
                table: "ServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTask_PaymentId",
                table: "ServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTask_ReplyId",
                table: "ServiceTask");

            migrationBuilder.DropIndex(
                name: "IX_Payment_ReplyId",
                table: "Payment");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "ServiceTask");

            migrationBuilder.DropColumn(
                name: "ReplyId",
                table: "Payment");

            migrationBuilder.AlterColumn<Guid>(
                name: "ServiceTaskId",
                table: "Payment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTask_ReplyId",
                table: "ServiceTask",
                column: "ReplyId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payment_ServiceTaskId",
                table: "Payment",
                column: "ServiceTaskId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Payment_ServiceTask_ServiceTaskId",
                table: "Payment",
                column: "ServiceTaskId",
                principalTable: "ServiceTask",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
