using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BIC_FHTW.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestableRoles",
                columns: table => new
                {
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    GuildId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestableRoles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    EmailString = table.Column<string>(type: "TEXT", nullable: false),
                    CourseOfStudyShortname = table.Column<string>(type: "TEXT", nullable: false),
                    CourseOfStudy = table.Column<string>(type: "TEXT", nullable: false),
                    Year = table.Column<int>(type: "INTEGER", nullable: false),
                    Semester = table.Column<int>(type: "INTEGER", nullable: false),
                    Association = table.Column<char>(type: "TEXT", nullable: false),
                    Group = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.EmailString);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StudentMail = table.Column<string>(type: "TEXT", nullable: true),
                    ActivationToken = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.DiscordUserId);
                    table.ForeignKey(
                        name: "FK_Users_Students_StudentMail",
                        column: x => x.StudentMail,
                        principalTable: "Students",
                        principalColumn: "EmailString");
                });

            migrationBuilder.CreateTable(
                name: "DiscordUserRoles",
                columns: table => new
                {
                    DiscordUserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscordUserRoles", x => new { x.DiscordUserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_DiscordUserRoles_RequestableRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "RequestableRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscordUserRoles_Users_DiscordUserId",
                        column: x => x.DiscordUserId,
                        principalTable: "Users",
                        principalColumn: "DiscordUserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscordUserRoles_RoleId",
                table: "DiscordUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Students_EmailString",
                table: "Students",
                column: "EmailString",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_DiscordUserId",
                table: "Users",
                column: "DiscordUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_StudentMail",
                table: "Users",
                column: "StudentMail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DiscordUserRoles");

            migrationBuilder.DropTable(
                name: "RequestableRoles");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Students");
        }
    }
}
