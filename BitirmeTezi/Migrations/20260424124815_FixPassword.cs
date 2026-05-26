using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitirmeTezi.Migrations
{
    /// <inheritdoc />
    public partial class FixPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StudentEnrollments_Courses_CourseId",
                table: "StudentEnrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgress_Courses_CourseId",
                table: "StudentProgress");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProgress_Lessons_LessonId",
                table: "StudentProgress");

            migrationBuilder.DropTable(
                name: "WritingQuestionTranslations");

            migrationBuilder.DropTable(
                name: "WritingQuestions");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgress_CourseId",
                table: "StudentProgress");

            migrationBuilder.DropIndex(
                name: "IX_StudentProgress_LessonId",
                table: "StudentProgress");

            migrationBuilder.DropIndex(
                name: "IX_StudentEnrollments_CourseId",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "AIFeedBack",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "ListeningStudentAnswer",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "SpeakingStudentAnswer",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "WritingStudentAnswer",
                table: "StudentProgress");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Students",
                newName: "PasswordHash");

            migrationBuilder.RenameColumn(
                name: "WritingQuestionId",
                table: "StudentProgress",
                newName: "TotalQuestions");

            migrationBuilder.RenameColumn(
                name: "SpeakingQuestionId",
                table: "StudentProgress",
                newName: "Skill");

            migrationBuilder.RenameColumn(
                name: "ListeningQuestionId",
                table: "StudentProgress",
                newName: "Level");

            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "StudentProgress",
                newName: "CorrectAnswers");

            migrationBuilder.RenameColumn(
                name: "AIScore",
                table: "StudentProgress",
                newName: "AverageScore");

            migrationBuilder.RenameColumn(
                name: "CourseId",
                table: "StudentEnrollments",
                newName: "Skill");

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Students",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentProgress",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<double>(
                name: "AIScore",
                table: "StudentEnrollments",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "StudentEnrollments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsCorrect",
                table: "StudentEnrollments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "StudentEnrollments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StudentAnswerText",
                table: "StudentEnrollments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentProgress");

            migrationBuilder.DropColumn(
                name: "AIScore",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "IsCorrect",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "StudentEnrollments");

            migrationBuilder.DropColumn(
                name: "StudentAnswerText",
                table: "StudentEnrollments");

            migrationBuilder.RenameColumn(
                name: "PasswordHash",
                table: "Students",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "TotalQuestions",
                table: "StudentProgress",
                newName: "WritingQuestionId");

            migrationBuilder.RenameColumn(
                name: "Skill",
                table: "StudentProgress",
                newName: "SpeakingQuestionId");

            migrationBuilder.RenameColumn(
                name: "Level",
                table: "StudentProgress",
                newName: "ListeningQuestionId");

            migrationBuilder.RenameColumn(
                name: "CorrectAnswers",
                table: "StudentProgress",
                newName: "LessonId");

            migrationBuilder.RenameColumn(
                name: "AverageScore",
                table: "StudentProgress",
                newName: "AIScore");

            migrationBuilder.RenameColumn(
                name: "Skill",
                table: "StudentEnrollments",
                newName: "CourseId");

            migrationBuilder.AddColumn<string>(
                name: "AIFeedBack",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "StudentProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ListeningStudentAnswer",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SpeakingStudentAnswer",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WritingStudentAnswer",
                table: "StudentProgress",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WritingQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    CorrectAnswer = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrammarTag = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WritingQuestions_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WritingQuestionTranslations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WritingQuestionId = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TranslatedText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WritingQuestionTranslations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WritingQuestionTranslations_WritingQuestions_WritingQuestionId",
                        column: x => x.WritingQuestionId,
                        principalTable: "WritingQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_CourseId",
                table: "StudentProgress",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProgress_LessonId",
                table: "StudentProgress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentEnrollments_CourseId",
                table: "StudentEnrollments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_WritingQuestions_LessonId",
                table: "WritingQuestions",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_WritingQuestionTranslations_WritingQuestionId_LanguageCode",
                table: "WritingQuestionTranslations",
                columns: new[] { "WritingQuestionId", "LanguageCode" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentEnrollments_Courses_CourseId",
                table: "StudentEnrollments",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgress_Courses_CourseId",
                table: "StudentProgress",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProgress_Lessons_LessonId",
                table: "StudentProgress",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
