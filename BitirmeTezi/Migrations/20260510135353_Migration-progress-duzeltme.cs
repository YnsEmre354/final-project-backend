using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BitirmeTezi.Migrations
{
    /// <inheritdoc />
    public partial class Migrationprogressduzeltme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           /* migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "StudentProgress");*/

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "StudentProgress",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "AverageScore",
                table: "StudentProgress",
                type: "real",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

          /*  migrationBuilder.AddColumn<bool>(
                name: "IsPassed",
                table: "StudentProgress",
                type: "bit",
                nullable: false,
                defaultValue: false);*/

          /*  migrationBuilder.AddColumn<float>(
                name: "ProgressPercentage",
                table: "StudentProgress",
                type: "real",
                nullable: false,
                defaultValue: 0f);*/

           /* migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "StudentProgress",
                type: "int",
                nullable: false,
                defaultValue: 0);*/
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          /*  migrationBuilder.DropColumn(
                name: "IsPassed",
                table: "StudentProgress");*/

            /*migrationBuilder.DropColumn(
                name: "ProgressPercentage",
                table: "StudentProgress");*/

           /* migrationBuilder.DropColumn(
                name: "Status",
                table: "StudentProgress");*/

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedDate",
                table: "StudentProgress",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<double>(
                name: "AverageScore",
                table: "StudentProgress",
                type: "float",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "real");

           /* migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "StudentProgress",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));*/
        }
    }
}
