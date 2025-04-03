using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialist3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specialist_Users_Id",
                table: "Specialist");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_User_Email",
                table: "User",
                column: "Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialist_User_Id",
                table: "Specialist",
                column: "Id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specialist_User_Id",
                table: "Specialist");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_User_Email",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialist_Users_Id",
                table: "Specialist",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
