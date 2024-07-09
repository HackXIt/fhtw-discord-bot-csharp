using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIC_FHTW.Database.Migrations
{
    /// <inheritdoc />
    public partial class MoveMailToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Students_StudentMail",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "StudentMail",
                table: "Users",
                newName: "StudentUid");

            migrationBuilder.RenameIndex(
                name: "IX_Users_StudentMail",
                table: "Users",
                newName: "IX_Users_StudentUid");

            migrationBuilder.RenameColumn(
                name: "EmailString",
                table: "Students",
                newName: "UID");

            migrationBuilder.RenameIndex(
                name: "IX_Students_EmailString",
                table: "Students",
                newName: "IX_Students_UID");

            migrationBuilder.AddColumn<string>(
                name: "ActivationMail",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Students_StudentUid",
                table: "Users",
                column: "StudentUid",
                principalTable: "Students",
                principalColumn: "UID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Students_StudentUid",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ActivationMail",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "StudentUid",
                table: "Users",
                newName: "StudentMail");

            migrationBuilder.RenameIndex(
                name: "IX_Users_StudentUid",
                table: "Users",
                newName: "IX_Users_StudentMail");

            migrationBuilder.RenameColumn(
                name: "UID",
                table: "Students",
                newName: "EmailString");

            migrationBuilder.RenameIndex(
                name: "IX_Students_UID",
                table: "Students",
                newName: "IX_Students_EmailString");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Students_StudentMail",
                table: "Users",
                column: "StudentMail",
                principalTable: "Students",
                principalColumn: "EmailString");
        }
    }
}
