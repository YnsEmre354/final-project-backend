using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitirmeTezi.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDailyScoreMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserDailyScores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<double>(type: "float", nullable: false),
                    SkillType = table.Column<int>(type: "int", nullable: false),
                    LevelType = table.Column<int>(type: "int", nullable: false),
                    EarnDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyScores", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDailyScores");
        }
    }
}
