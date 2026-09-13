
CREATE TRIGGER TR_UserAnswers_PreventDuplicateQuestion
ON UserAnswers
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM UserAnswers ua
        INNER JOIN Answers a
            ON a.AnswerId = ua.AnswerId
        INNER JOIN inserted i
            ON i.UserProfileId = ua.UserProfileId
        INNER JOIN Answers ia
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