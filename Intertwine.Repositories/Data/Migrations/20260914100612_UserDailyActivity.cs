using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations
{
    /// <inheritdoc />
    public partial class UserDailyActivity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserProfileId",
                table: "UserAnswers");

            migrationBuilder.CreateTable(
                name: "UserDailyActivities",
                columns: table => new
                {
                    UserDailyActivityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserProfileId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    NonDailyQuestionsAnswered = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DailyQuestionCreateOrUpdateUsed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDailyActivities", x => x.UserDailyActivityId);
                    table.ForeignKey(
                        name: "FK_UserDailyActivities_UserProfiles_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "UserProfiles",
                        principalColumn: "UserProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserProfileId_AnswerId",
                table: "UserAnswers",
                columns: new[] { "UserProfileId", "AnswerId" });

            migrationBuilder.CreateIndex(
                name: "IX_UserDailyActivities_UserProfileId_Date",
                table: "UserDailyActivities",
                columns: new[] { "UserProfileId", "Date" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserDailyActivities");

            migrationBuilder.DropIndex(
                name: "IX_UserAnswers_UserProfileId_AnswerId",
                table: "UserAnswers");

            migrationBuilder.CreateIndex(
                name: "IX_UserAnswers_UserProfileId",
                table: "UserAnswers",
                column: "UserProfileId");
        }
    }
}
