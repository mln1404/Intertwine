using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAnswerUniquenessTrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE TRIGGER [dbo].[TR_UserAnswers_PreventDuplicateQuestion]
                ON [dbo].[UserAnswers]
                AFTER INSERT, UPDATE
                AS
                BEGIN
                    SET NOCOUNT ON;

                    IF EXISTS
                    (
                        SELECT 1
                        FROM [dbo].[UserAnswers] AS ua
                        INNER JOIN [dbo].[Answers] AS a
                            ON a.AnswerId = ua.AnswerId
                        INNER JOIN inserted AS i
                            ON i.UserProfileId = ua.UserProfileId
                        INNER JOIN [dbo].[Answers] AS ia
                            ON ia.AnswerId = i.AnswerId
                        WHERE a.QuestionId = ia.QuestionId
                            AND ua.UserAnswerId <> i.UserAnswerId
                    )
                    BEGIN
                        THROW 50001,
                            'A user can only have one current answer for each question.',
                            1;
                    END
                END;
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER [dbo].[TR_UserAnswers_PreventDuplicateQuestion];");
        }
    }
}
