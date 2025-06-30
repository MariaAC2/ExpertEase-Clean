using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangedTaskReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Review_ServiceTaskId",
                table: "Review");

            migrationBuilder.CreateIndex(
                name: "IX_Review_ServiceTaskId",
                table: "Review",
                column: "ServiceTaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Review_ServiceTaskId",
                table: "Review");

            migrationBuilder.CreateIndex(
                name: "IX_Review_ServiceTaskId",
                table: "Review",
                column: "ServiceTaskId",
                unique: true);
        }
    }
}
