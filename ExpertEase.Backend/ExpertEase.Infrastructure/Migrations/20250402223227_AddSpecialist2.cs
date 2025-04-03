using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExpertEase.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialist2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Specialists_User_Id",
                table: "Specialists");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_User_Email",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialists",
                table: "Specialists");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Specialists",
                newName: "Specialist");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialist",
                table: "Specialist",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialist_Users_Id",
                table: "Specialist",
                column: "Id",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialist",
                table: "Specialist");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Specialist",
                newName: "Specialists");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_User_Email",
                table: "User",
                column: "Email");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialists",
                table: "Specialists",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Specialists_User_Id",
                table: "Specialists",
                column: "Id",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
