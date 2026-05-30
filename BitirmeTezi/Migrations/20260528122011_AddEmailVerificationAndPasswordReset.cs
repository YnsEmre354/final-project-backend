using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitirmeTezi.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationAndPasswordReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "UserDailyScores",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "Students",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "Students",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Students",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "Students",
                type: "datetime2",
                nullable: true);

            // Mevcut kullanıcılar doğrulanmış kabul edilir.
            // Yeni kayıtlar default 0 (false) ile başlayacak.
            migrationBuilder.Sql("UPDATE Students SET IsEmailVerified = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiry",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "Students");

            migrationBuilder.AlterColumn<double>(
                name: "Points",
                table: "UserDailyScores",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
