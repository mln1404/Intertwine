using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations
{
    /// <inheritdoc />
    public partial class BaseEntityAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateNow",
                table: "DailyQuestions");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "UserProfiles",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "UserProfiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "UserAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "UserAnswers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "UserAnswers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "UserAnswers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "Questions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UpdatedBy",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Date",
                table: "DailyQuestions",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserProfiles");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "UserAnswers");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "DailyQuestions");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateNow",
                table: "DailyQuestions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
