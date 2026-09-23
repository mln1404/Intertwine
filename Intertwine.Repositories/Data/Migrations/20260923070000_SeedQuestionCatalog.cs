using Intertwine.Repositories.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Intertwine.Repositories.Data.Migrations;

/// <summary>
/// Seeds the initial question catalog, answer choices, categories, and question-category links.
/// </summary>
[DbContext(typeof(IntertwineDbContext))]
[Migration("20260923070000_SeedQuestionCatalog")]
public partial class SeedQuestionCatalog : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
"""
MERGE INTO Categories AS target
USING
(
    VALUES
        (N'Personality', N'#6C5CE7'),
        (N'Relationships', N'#E84393'),
        (N'Love & Romance', N'#FF7675'),
        (N'Friendship', N'#0984E3'),
        (N'Family', N'#00B894'),
        (N'Life & Goals', N'#FDCB6E'),
        (N'Values & Beliefs', N'#A29BFE'),
        (N'Interests & Hobbies', N'#E17055'),
        (N'Lifestyle', N'#00CEC9'),
        (N'Food & Drink', N'#E67E22'),
        (N'Travel & Adventure', N'#2D98DA'),
        (N'Career & Ambition', N'#3867D6'),
        (N'Childhood & Memories', N'#F78FB3'),
        (N'Fun & Random', N'#FDA7DF'),
        (N'Deep & Meaningful', N'#574B90')
) AS source (CategoryName, Color)
    ON target.CategoryName = source.CategoryName
WHEN MATCHED THEN
    UPDATE SET Color = source.Color
WHEN NOT MATCHED BY TARGET THEN
    INSERT (CategoryName, Color, CreatedBy)
    VALUES (source.CategoryName, source.Color, N'SeedQuestionCatalog');

DECLARE @ExistingQuestionCount1 int =
(
    SELECT COUNT(DISTINCT QuestionTitle)
    FROM Questions
    WHERE QuestionTitle IN
    (
        N'Childhood Happiness',
        N'Growing Up',
        N'Nostalgia',
        N'Childhood Traditions',
        N'Old Dreams',
        N'Comfort Food',
        N'Food Adventure',
        N'Cooking Together',
        N'Perfect Meal',
        N'Food and Memories',
        N'Spontaneous Fun',
        N'Sense of Humor',
        N'Friendly Competition',
        N'Random Adventure',
        N'Embarrassing Moments',
        N'Shared Hobbies',
        N'Learning Together',
        N'Personal Interests',
        N'Trying New Things',
        N'Passion'
    )
);

IF @ExistingQuestionCount1 = 0
BEGIN
    /* =========================================================
           CHILDHOOD & MEMORIES
           ========================================================= */
    
        INSERT INTO Questions
        (
            QuestionTitle,
            FullQuestion
        )
        VALUES
        (
            'Childhood Happiness',
            'What kind of childhood memory would you most want to share with someone you love?'
        ),
        (
            'Growing Up',
            'Which experience from growing up had the biggest influence on who you are today?'
        ),
        (
            'Nostalgia',
            'If you could relive one part of your childhood for a day, what would you choose?'
        ),
        (
            'Childhood Traditions',
            'Which childhood tradition would you most want to continue with the people you love?'
        ),
        (
            'Old Dreams',
            'What did you dream of becoming when you were younger?'
        );
    
    
        /* =========================================================
           FOOD & DRINK
           ========================================================= */
    
        INSERT INTO Questions
        (
            QuestionTitle,
            FullQuestion
        )
        VALUES
        (
            'Comfort Food',
            'What kind of food would you want waiting for you after a difficult day?'
        ),
        (
            'Food Adventure',
            'If someone you love wanted to try a completely unfamiliar food, how would you react?'
        ),
        (
            'Cooking Together',
            'What sounds most enjoyable about cooking with someone you care about?'
        ),
        (
            'Perfect Meal',
            'What would your ideal meal with someone special look like?'
        ),
        (
            'Food and Memories',
            'Which kind of food is most likely to remind you of a happy memory?'
        );
    
    
        /* =========================================================
           FUN & RANDOM
           ========================================================= */
    
        INSERT INTO Questions
        (
            QuestionTitle,
            FullQuestion
        )
        VALUES
        (
            'Spontaneous Fun',
            'Someone you care about suddenly suggests doing something ridiculous together. What do you do?'
        ),
        (
            'Sense of Humor',
            'What kind of humor are you most likely to enjoy with someone close to you?'
        ),
        (
            'Friendly Competition',
            'If you and someone you care about became unexpectedly competitive over a game, what would you do?'
        ),
        (
            'Random Adventure',
            'You have three completely free hours with someone you like. What sounds the most fun?'
        ),
        (
            'Embarrassing Moments',
            'If someone you care about embarrasses themselves in public, what would you most likely do?'
        );
    
    
        /* =========================================================
           INTERESTS & HOBBIES
           ========================================================= */
    
        INSERT INTO Questions
        (
            QuestionTitle,
            FullQuestion
        )
        VALUES
        (
            'Shared Hobbies',
            'How important is it to you that a partner or close friend shares your hobbies?'
        ),
        (
            'Learning Together',
            'Someone you care about becomes interested in a hobby you know nothing about. What sounds most appealing?'
        ),
        (
            'Personal Interests',
            'How do you feel about having hobbies that your partner does not participate in?'
        ),
        (
            'Trying New Things',
            'If you could learn one completely new skill with someone you care about, what would you prefer?'
        ),
        (
            'Passion',
            'What makes you most excited to share one of your interests with another person?'
        );
    
    
        /* =========================================================
           ANSWERS
           ========================================================= */
    
        /* ---------------------------------------------------------
           CHILDHOOD HAPPINESS
           Childhood & Memories + Relationships
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A funny family moment'
        FROM Questions WHERE QuestionTitle = 'Childhood Happiness'
        UNION ALL
        SELECT QuestionId, 'A special adventure with friends'
        FROM Questions WHERE QuestionTitle = 'Childhood Happiness'
        UNION ALL
        SELECT QuestionId, 'A peaceful moment at home'
        FROM Questions WHERE QuestionTitle = 'Childhood Happiness'
        UNION ALL
        SELECT QuestionId, 'A moment when I felt especially proud'
        FROM Questions WHERE QuestionTitle = 'Childhood Happiness';
    
    
        /* Growing Up */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'My family'
        FROM Questions WHERE QuestionTitle = 'Growing Up'
        UNION ALL
        SELECT QuestionId, 'My friendships'
        FROM Questions WHERE QuestionTitle = 'Growing Up'
        UNION ALL
        SELECT QuestionId, 'School and teachers'
        FROM Questions WHERE QuestionTitle = 'Growing Up'
        UNION ALL
        SELECT QuestionId, 'Challenges I had to overcome'
        FROM Questions WHERE QuestionTitle = 'Growing Up';
    
    
        /* Nostalgia */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Playing with friends'
        FROM Questions WHERE QuestionTitle = 'Nostalgia'
        UNION ALL
        SELECT QuestionId, 'Family gatherings'
        FROM Questions WHERE QuestionTitle = 'Nostalgia'
        UNION ALL
        SELECT QuestionId, 'School days'
        FROM Questions WHERE QuestionTitle = 'Nostalgia'
        UNION ALL
        SELECT QuestionId, 'A favorite place from childhood'
        FROM Questions WHERE QuestionTitle = 'Nostalgia';
    
    
        /* Childhood Traditions */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Family celebrations'
        FROM Questions WHERE QuestionTitle = 'Childhood Traditions'
        UNION ALL
        SELECT QuestionId, 'Holiday traditions'
        FROM Questions WHERE QuestionTitle = 'Childhood Traditions'
        UNION ALL
        SELECT QuestionId, 'Weekend routines'
        FROM Questions WHERE QuestionTitle = 'Childhood Traditions'
        UNION ALL
        SELECT QuestionId, 'Special meals together'
        FROM Questions WHERE QuestionTitle = 'Childhood Traditions';
    
    
        /* Old Dreams */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Something creative'
        FROM Questions WHERE QuestionTitle = 'Old Dreams'
        UNION ALL
        SELECT QuestionId, 'A career where I could help people'
        FROM Questions WHERE QuestionTitle = 'Old Dreams'
        UNION ALL
        SELECT QuestionId, 'Something adventurous'
        FROM Questions WHERE QuestionTitle = 'Old Dreams'
        UNION ALL
        SELECT QuestionId, 'Something impressive or successful'
        FROM Questions WHERE QuestionTitle = 'Old Dreams';
    
    
        /* ---------------------------------------------------------
           FOOD & DRINK
           --------------------------------------------------------- */
    
        /* Comfort Food */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A home-cooked meal'
        FROM Questions WHERE QuestionTitle = 'Comfort Food'
        UNION ALL
        SELECT QuestionId, 'Fast food or takeout'
        FROM Questions WHERE QuestionTitle = 'Comfort Food'
        UNION ALL
        SELECT QuestionId, 'My favorite dessert'
        FROM Questions WHERE QuestionTitle = 'Comfort Food'
        UNION ALL
        SELECT QuestionId, 'Whatever meal reminds me of home'
        FROM Questions WHERE QuestionTitle = 'Comfort Food';
    
    
        /* Food Adventure */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Try it together immediately'
        FROM Questions WHERE QuestionTitle = 'Food Adventure'
        UNION ALL
        SELECT QuestionId, 'Ask what it tastes like first'
        FROM Questions WHERE QuestionTitle = 'Food Adventure'
        UNION ALL
        SELECT QuestionId, 'Try a small portion'
        FROM Questions WHERE QuestionTitle = 'Food Adventure'
        UNION ALL
        SELECT QuestionId, 'I would probably stick with something familiar'
        FROM Questions WHERE QuestionTitle = 'Food Adventure';
    
    
        /* Cooking Together */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Creating a complicated meal together'
        FROM Questions WHERE QuestionTitle = 'Cooking Together'
        UNION ALL
        SELECT QuestionId, 'Making something simple and talking'
        FROM Questions WHERE QuestionTitle = 'Cooking Together'
        UNION ALL
        SELECT QuestionId, 'Trying to recreate something from a restaurant'
        FROM Questions WHERE QuestionTitle = 'Cooking Together'
        UNION ALL
        SELECT QuestionId, 'Making dessert together'
        FROM Questions WHERE QuestionTitle = 'Cooking Together';
    
    
        /* Perfect Meal */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A romantic dinner'
        FROM Questions WHERE QuestionTitle = 'Perfect Meal'
        UNION ALL
        SELECT QuestionId, 'A casual meal at our favorite place'
        FROM Questions WHERE QuestionTitle = 'Perfect Meal'
        UNION ALL
        SELECT QuestionId, 'A huge feast with friends'
        FROM Questions WHERE QuestionTitle = 'Perfect Meal'
        UNION ALL
        SELECT QuestionId, 'Cooking something together at home'
        FROM Questions WHERE QuestionTitle = 'Perfect Meal';
    
    
        /* Food and Memories */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Something my family used to cook'
        FROM Questions WHERE QuestionTitle = 'Food and Memories'
        UNION ALL
        SELECT QuestionId, 'A favorite childhood snack'
        FROM Questions WHERE QuestionTitle = 'Food and Memories'
        UNION ALL
        SELECT QuestionId, 'Food from a memorable trip'
        FROM Questions WHERE QuestionTitle = 'Food and Memories'
        UNION ALL
        SELECT QuestionId, 'Something associated with a special person'
        FROM Questions WHERE QuestionTitle = 'Food and Memories';
    
    
        /* ---------------------------------------------------------
           FUN & RANDOM
           --------------------------------------------------------- */
    
        /* Spontaneous Fun */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Absolutely. Let''s do it!'
        FROM Questions WHERE QuestionTitle = 'Spontaneous Fun'
        UNION ALL
        SELECT QuestionId, 'I''m interested, but tell me the plan'
        FROM Questions WHERE QuestionTitle = 'Spontaneous Fun'
        UNION ALL
        SELECT QuestionId, 'I''ll probably join if everyone else does'
        FROM Questions WHERE QuestionTitle = 'Spontaneous Fun'
        UNION ALL
        SELECT QuestionId, 'I would rather do something familiar'
        FROM Questions WHERE QuestionTitle = 'Spontaneous Fun';
    
    
        /* Sense of Humor */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Playful teasing'
        FROM Questions WHERE QuestionTitle = 'Sense of Humor'
        UNION ALL
        SELECT QuestionId, 'Dry or sarcastic humor'
        FROM Questions WHERE QuestionTitle = 'Sense of Humor'
        UNION ALL
        SELECT QuestionId, 'Ridiculous and absurd humor'
        FROM Questions WHERE QuestionTitle = 'Sense of Humor'
        UNION ALL
        SELECT QuestionId, 'Wholesome and silly humor'
        FROM Questions WHERE QuestionTitle = 'Sense of Humor';
    
    
        /* Friendly Competition */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'I want to win!'
        FROM Questions WHERE QuestionTitle = 'Friendly Competition'
        UNION ALL
        SELECT QuestionId, 'I enjoy the competition but keep it light'
        FROM Questions WHERE QuestionTitle = 'Friendly Competition'
        UNION ALL
        SELECT QuestionId, 'I care more about having fun'
        FROM Questions WHERE QuestionTitle = 'Friendly Competition'
        UNION ALL
        SELECT QuestionId, 'I would rather cooperate than compete'
        FROM Questions WHERE QuestionTitle = 'Friendly Competition';
    
    
        /* Random Adventure */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Explore somewhere neither of us has been'
        FROM Questions WHERE QuestionTitle = 'Random Adventure'
        UNION ALL
        SELECT QuestionId, 'Find somewhere interesting to eat'
        FROM Questions WHERE QuestionTitle = 'Random Adventure'
        UNION ALL
        SELECT QuestionId, 'Play games or do something silly'
        FROM Questions WHERE QuestionTitle = 'Random Adventure'
        UNION ALL
        SELECT QuestionId, 'Relax somewhere and talk'
        FROM Questions WHERE QuestionTitle = 'Random Adventure';
    
    
        /* Embarrassing Moments */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Laugh with them'
        FROM Questions WHERE QuestionTitle = 'Embarrassing Moments'
        UNION ALL
        SELECT QuestionId, 'Help them get out of the situation'
        FROM Questions WHERE QuestionTitle = 'Embarrassing Moments'
        UNION ALL
        SELECT QuestionId, 'Pretend nothing happened'
        FROM Questions WHERE QuestionTitle = 'Embarrassing Moments'
        UNION ALL
        SELECT QuestionId, 'Probably tease them about it later'
        FROM Questions WHERE QuestionTitle = 'Embarrassing Moments';
    
    
        /* ---------------------------------------------------------
           INTERESTS & HOBBIES
           --------------------------------------------------------- */
    
        /* Shared Hobbies */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Very important — I want lots of shared interests'
        FROM Questions WHERE QuestionTitle = 'Shared Hobbies'
        UNION ALL
        SELECT QuestionId, 'Nice to have, but not necessary'
        FROM Questions WHERE QuestionTitle = 'Shared Hobbies'
        UNION ALL
        SELECT QuestionId, 'I prefer having some hobbies together and some separately'
        FROM Questions WHERE QuestionTitle = 'Shared Hobbies'
        UNION ALL
        SELECT QuestionId, 'Not important — we can enjoy completely different things'
        FROM Questions WHERE QuestionTitle = 'Shared Hobbies';
    
    
        /* Learning Together */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Learn it together'
        FROM Questions WHERE QuestionTitle = 'Learning Together'
        UNION ALL
        SELECT QuestionId, 'Let them teach me'
        FROM Questions WHERE QuestionTitle = 'Learning Together'
        UNION ALL
        SELECT QuestionId, 'Encourage them while they explore it themselves'
        FROM Questions WHERE QuestionTitle = 'Learning Together'
        UNION ALL
        SELECT QuestionId, 'Find a related hobby we can both enjoy'
        FROM Questions WHERE QuestionTitle = 'Learning Together';
    
    
        /* Personal Interests */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'I love having completely separate hobbies'
        FROM Questions WHERE QuestionTitle = 'Personal Interests'
        UNION ALL
        SELECT QuestionId, 'Some separate hobbies are healthy'
        FROM Questions WHERE QuestionTitle = 'Personal Interests'
        UNION ALL
        SELECT QuestionId, 'I prefer sharing most hobbies'
        FROM Questions WHERE QuestionTitle = 'Personal Interests'
        UNION ALL
        SELECT QuestionId, 'It depends on how much time we have together'
        FROM Questions WHERE QuestionTitle = 'Personal Interests';
    
    
        /* Trying New Things */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Something creative'
        FROM Questions WHERE QuestionTitle = 'Trying New Things'
        UNION ALL
        SELECT QuestionId, 'Something physically challenging'
        FROM Questions WHERE QuestionTitle = 'Trying New Things'
        UNION ALL
        SELECT QuestionId, 'Something practical'
        FROM Questions WHERE QuestionTitle = 'Trying New Things'
        UNION ALL
        SELECT QuestionId, 'Something completely unusual'
        FROM Questions WHERE QuestionTitle = 'Trying New Things';
    
    
        /* Passion */
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Being able to share something I love'
        FROM Questions WHERE QuestionTitle = 'Passion'
        UNION ALL
        SELECT QuestionId, 'Having someone genuinely curious about it'
        FROM Questions WHERE QuestionTitle = 'Passion'
        UNION ALL
        SELECT QuestionId, 'Being able to teach someone'
        FROM Questions WHERE QuestionTitle = 'Passion'
        UNION ALL
        SELECT QuestionId, 'Discovering something we both enjoy'
        FROM Questions WHERE QuestionTitle = 'Passion';
    
    
        /* =========================================================
           QUESTION → CATEGORY LINKS
           ========================================================= */
    
        INSERT INTO QuestionCategories
        (
            QuestionId,
            CategoryId
        )
        SELECT
            q.QuestionId,
            c.CategoryId
        FROM Questions q
        INNER JOIN Categories c
            ON
                /* Childhood & Memories */
                (
                    q.QuestionTitle = 'Childhood Happiness'
                    AND c.CategoryName IN
                    (
                        'Childhood & Memories',
                        'Relationships'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Growing Up'
                    AND c.CategoryName IN
                    (
                        'Childhood & Memories',
                        'Personality',
                        'Values & Beliefs'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Nostalgia'
                    AND c.CategoryName IN
                    (
                        'Childhood & Memories',
                        'Fun & Random'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Childhood Traditions'
                    AND c.CategoryName IN
                    (
                        'Childhood & Memories',
                        'Family'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Old Dreams'
                    AND c.CategoryName IN
                    (
                        'Childhood & Memories',
                        'Career & Ambition',
                        'Life & Goals'
                    )
                )
    
                OR
    
                /* Food & Drink */
                (
                    q.QuestionTitle = 'Comfort Food'
                    AND c.CategoryName IN
                    (
                        'Food & Drink',
                        'Lifestyle'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Food Adventure'
                    AND c.CategoryName IN
                    (
                        'Food & Drink',
                        'Travel & Adventure'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Cooking Together'
                    AND c.CategoryName IN
                    (
                        'Food & Drink',
                        'Relationships',
                        'Love & Romance'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Perfect Meal'
                    AND c.CategoryName IN
                    (
                        'Food & Drink',
                        'Love & Romance',
                        'Lifestyle'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Food and Memories'
                    AND c.CategoryName IN
                    (
                        'Food & Drink',
                        'Childhood & Memories',
                        'Family'
                    )
                )
    
                OR
    
                /* Fun & Random */
                (
                    q.QuestionTitle = 'Spontaneous Fun'
                    AND c.CategoryName IN
                    (
                        'Fun & Random',
                        'Lifestyle',
                        'Relationships'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Sense of Humor'
                    AND c.CategoryName IN
                    (
                        'Fun & Random',
                        'Personality',
                        'Relationships'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Friendly Competition'
                    AND c.CategoryName IN
                    (
                        'Fun & Random',
                        'Friendship',
                        'Personality'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Random Adventure'
                    AND c.CategoryName IN
                    (
                        'Fun & Random',
                        'Travel & Adventure',
                        'Lifestyle'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Embarrassing Moments'
                    AND c.CategoryName IN
                    (
                        'Fun & Random',
                        'Relationships',
                        'Friendship'
                    )
                )
    
                OR
    
                /* Interests & Hobbies */
                (
                    q.QuestionTitle = 'Shared Hobbies'
                    AND c.CategoryName IN
                    (
                        'Interests & Hobbies',
                        'Relationships',
                        'Lifestyle'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Learning Together'
                    AND c.CategoryName IN
                    (
                        'Interests & Hobbies',
                        'Relationships',
                        'Fun & Random'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Personal Interests'
                    AND c.CategoryName IN
                    (
                        'Interests & Hobbies',
                        'Lifestyle',
                        'Relationships'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Trying New Things'
                    AND c.CategoryName IN
                    (
                        'Interests & Hobbies',
                        'Travel & Adventure',
                        'Fun & Random'
                    )
                )
                OR
                (
                    q.QuestionTitle = 'Passion'
                    AND c.CategoryName IN
                    (
                        'Interests & Hobbies',
                        'Personality',
                        'Relationships'
                    )
                );

    UPDATE Questions
    SET CreatedBy = N'SeedQuestionCatalog'
    WHERE QuestionTitle IN
    (
        N'Childhood Happiness',
        N'Growing Up',
        N'Nostalgia',
        N'Childhood Traditions',
        N'Old Dreams',
        N'Comfort Food',
        N'Food Adventure',
        N'Cooking Together',
        N'Perfect Meal',
        N'Food and Memories',
        N'Spontaneous Fun',
        N'Sense of Humor',
        N'Friendly Competition',
        N'Random Adventure',
        N'Embarrassing Moments',
        N'Shared Hobbies',
        N'Learning Together',
        N'Personal Interests',
        N'Trying New Things',
        N'Passion'
    );
END
ELSE IF @ExistingQuestionCount1 <> 20
BEGIN
    THROW 51000, 'Question seed batch 1 is partially present. Complete or remove the partial batch before applying this migration.', 1;
END;

DECLARE @ExistingQuestionCount2 int =
(
    SELECT COUNT(DISTINCT QuestionTitle)
    FROM Questions
    WHERE QuestionTitle IN
    (
        N'Moving for Love',
        N'Handling Conflict',
        N'Quality Time',
        N'Financial Priorities',
        N'Ambition and Partnership',
        N'Social Life',
        N'Family Boundaries',
        N'Affection',
        N'Life Direction',
        N'Adventure',
        N'Independence',
        N'Emotional Support',
        N'Money and Relationships',
        N'Future Family',
        N'Success',
        N'Everyday Compatibility',
        N'Trust',
        N'Change',
        N'Romance',
        N'Meaningful Life'
    )
);

IF @ExistingQuestionCount2 = 0
BEGIN
    /* =========================================================
           QUESTIONS
           ========================================================= */
    
        INSERT INTO Questions
        (
            QuestionTitle,
            FullQuestion
        )
        VALUES
    
        -- 1
        (
            'Moving for Love',
            'Your partner receives an amazing career opportunity in another city. What would you hope the two of you do?'
        ),
    
        -- 2
        (
            'Handling Conflict',
            'When you and your partner strongly disagree about something important, what feels like the healthiest approach?'
        ),
    
        -- 3
        (
            'Quality Time',
            'What kind of time together makes you feel most connected to someone?'
        ),
    
        -- 4
        (
            'Financial Priorities',
            'If you suddenly had enough money to make one major improvement to your life, what would you prioritize?'
        ),
    
        -- 5
        (
            'Ambition and Partnership',
            'Your partner becomes extremely focused on achieving a major career goal. How would you ideally support each other?'
        ),
    
        -- 6
        (
            'Social Life',
            'Your partner wants to spend the weekend at a large gathering, but you would rather stay home. What sounds like the best compromise?'
        ),
    
        -- 7
        (
            'Family Boundaries',
            'If your family strongly disagreed with an important decision you and your partner made together, what should happen next?'
        ),
    
        -- 8
        (
            'Affection',
            'When you care deeply about someone, which way of expressing affection feels most natural to you?'
        ),
    
        -- 9
        (
            'Life Direction',
            'If you could choose your ideal life five years from now, which would matter most?'
        ),
    
        -- 10
        (
            'Adventure',
            'Your partner suggests taking a spontaneous trip with very little planning. What is your reaction?'
        ),
    
        -- 11
        (
            'Independence',
            'How much personal space do you think people in a healthy relationship should have?'
        ),
    
        -- 12
        (
            'Emotional Support',
            'When someone you love is having a difficult time, what do you think they need most from you?'
        ),
    
        -- 13
        (
            'Money and Relationships',
            'If you and your partner had very different spending habits, what would be the best way to handle it?'
        ),
    
        -- 14
        (
            'Future Family',
            'When imagining your future, how important is having a family or children to you?'
        ),
    
        -- 15
        (
            'Success',
            'Which would make you feel most successful in life?'
        ),
    
        -- 16
        (
            'Everyday Compatibility',
            'If you could choose your ideal ordinary Saturday with a partner, what would it look like?'
        ),
    
        -- 17
        (
            'Trust',
            'What is most important for building trust between two people?'
        ),
    
        -- 18
        (
            'Change',
            'Your partner suddenly realizes they want to completely change careers or pursue a very different life. How would you respond?'
        ),
    
        -- 19
        (
            'Romance',
            'What makes a long-term relationship continue to feel romantic?'
        ),
    
        -- 20
        (
            'Meaningful Life',
            'Which statement comes closest to what you believe makes a life worth living?'
        );
    
    
        /* =========================================================
           ANSWERS
           ========================================================= */
    
        /* ---------------------------------------------------------
           1. Moving for Love
           Categories:
           Love & Romance
           Career & Ambition
           Travel & Adventure
           Life & Goals
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Move together and embrace the opportunity'
        FROM Questions
        WHERE QuestionTitle = 'Moving for Love'
        UNION ALL
        SELECT QuestionId, 'Figure out a long-distance arrangement first'
        FROM Questions
        WHERE QuestionTitle = 'Moving for Love'
        UNION ALL
        SELECT QuestionId, 'Let the person take the opportunity independently'
        FROM Questions
        WHERE QuestionTitle = 'Moving for Love'
        UNION ALL
        SELECT QuestionId, 'Look for an opportunity that works for both people'
        FROM Questions
        WHERE QuestionTitle = 'Moving for Love';
    
    
        /* ---------------------------------------------------------
           2. Handling Conflict
           Relationships + Love & Romance + Values & Beliefs
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Talk about it calmly as soon as possible'
        FROM Questions
        WHERE QuestionTitle = 'Handling Conflict'
        UNION ALL
        SELECT QuestionId, 'Take some time apart before discussing it'
        FROM Questions
        WHERE QuestionTitle = 'Handling Conflict'
        UNION ALL
        SELECT QuestionId, 'Focus on understanding each other before solving it'
        FROM Questions
        WHERE QuestionTitle = 'Handling Conflict'
        UNION ALL
        SELECT QuestionId, 'Find a compromise even if neither person gets everything they want'
        FROM Questions
        WHERE QuestionTitle = 'Handling Conflict';
    
    
        /* ---------------------------------------------------------
           3. Quality Time
           Relationships + Love & Romance + Lifestyle
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Having deep conversations'
        FROM Questions
        WHERE QuestionTitle = 'Quality Time'
        UNION ALL
        SELECT QuestionId, 'Doing activities and adventures together'
        FROM Questions
        WHERE QuestionTitle = 'Quality Time'
        UNION ALL
        SELECT QuestionId, 'Quietly relaxing together'
        FROM Questions
        WHERE QuestionTitle = 'Quality Time'
        UNION ALL
        SELECT QuestionId, 'Going out and experiencing new places'
        FROM Questions
        WHERE QuestionTitle = 'Quality Time';
    
    
        /* ---------------------------------------------------------
           4. Financial Priorities
           Life & Goals + Career & Ambition + Lifestyle
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Invest or save it for the future'
        FROM Questions
        WHERE QuestionTitle = 'Financial Priorities'
        UNION ALL
        SELECT QuestionId, 'Use it to improve my home or lifestyle'
        FROM Questions
        WHERE QuestionTitle = 'Financial Priorities'
        UNION ALL
        SELECT QuestionId, 'Spend it on travel and experiences'
        FROM Questions
        WHERE QuestionTitle = 'Financial Priorities'
        UNION ALL
        SELECT QuestionId, 'Use it to pursue a business or career opportunity'
        FROM Questions
        WHERE QuestionTitle = 'Financial Priorities';
    
    
        /* ---------------------------------------------------------
           5. Ambition and Partnership
           Career & Ambition + Relationships + Love & Romance
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Encourage them and help however I can'
        FROM Questions
        WHERE QuestionTitle = 'Ambition and Partnership'
        UNION ALL
        SELECT QuestionId, 'Give them space to focus while maintaining our connection'
        FROM Questions
        WHERE QuestionTitle = 'Ambition and Partnership'
        UNION ALL
        SELECT QuestionId, 'Make sure both partners continue prioritizing the relationship'
        FROM Questions
        WHERE QuestionTitle = 'Ambition and Partnership'
        UNION ALL
        SELECT QuestionId, 'Adjust our plans together so both people can grow'
        FROM Questions
        WHERE QuestionTitle = 'Ambition and Partnership';
    
    
        /* ---------------------------------------------------------
           6. Social Life
           Friendship + Lifestyle + Relationships
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Go to the gathering together'
        FROM Questions
        WHERE QuestionTitle = 'Social Life'
        UNION ALL
        SELECT QuestionId, 'Spend part of the day together and part separately'
        FROM Questions
        WHERE QuestionTitle = 'Social Life'
        UNION ALL
        SELECT QuestionId, 'Take turns choosing what to do'
        FROM Questions
        WHERE QuestionTitle = 'Social Life'
        UNION ALL
        SELECT QuestionId, 'Stay home this time and attend the next event'
        FROM Questions
        WHERE QuestionTitle = 'Social Life';
    
    
        /* ---------------------------------------------------------
           7. Family Boundaries
           Family + Relationships + Values & Beliefs
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Stand together and make the decision independently'
        FROM Questions
        WHERE QuestionTitle = 'Family Boundaries'
        UNION ALL
        SELECT QuestionId, 'Try to understand the family''s concerns first'
        FROM Questions
        WHERE QuestionTitle = 'Family Boundaries'
        UNION ALL
        SELECT QuestionId, 'Find a compromise that respects everyone'
        FROM Questions
        WHERE QuestionTitle = 'Family Boundaries'
        UNION ALL
        SELECT QuestionId, 'Set clear boundaries if the disagreement continues'
        FROM Questions
        WHERE QuestionTitle = 'Family Boundaries';
    
    
        /* ---------------------------------------------------------
           8. Affection
           Love & Romance + Relationships
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Physical affection and closeness'
        FROM Questions
        WHERE QuestionTitle = 'Affection'
        UNION ALL
        SELECT QuestionId, 'Words of encouragement and reassurance'
        FROM Questions
        WHERE QuestionTitle = 'Affection'
        UNION ALL
        SELECT QuestionId, 'Doing thoughtful things for them'
        FROM Questions
        WHERE QuestionTitle = 'Affection'
        UNION ALL
        SELECT QuestionId, 'Spending meaningful time together'
        FROM Questions
        WHERE QuestionTitle = 'Affection';
    
    
        /* ---------------------------------------------------------
           9. Life Direction
           Life & Goals + Career & Ambition + Love & Romance
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A successful and fulfilling career'
        FROM Questions
        WHERE QuestionTitle = 'Life Direction'
        UNION ALL
        SELECT QuestionId, 'A loving and stable relationship'
        FROM Questions
        WHERE QuestionTitle = 'Life Direction'
        UNION ALL
        SELECT QuestionId, 'Freedom to explore and experience life'
        FROM Questions
        WHERE QuestionTitle = 'Life Direction'
        UNION ALL
        SELECT QuestionId, 'A comfortable and peaceful life'
        FROM Questions
        WHERE QuestionTitle = 'Life Direction';
    
    
        /* ---------------------------------------------------------
           10. Adventure
           Travel & Adventure + Lifestyle + Relationships
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Absolutely. Let''s go!'
        FROM Questions
        WHERE QuestionTitle = 'Adventure'
        UNION ALL
        SELECT QuestionId, 'I''m interested, but let''s plan the important details'
        FROM Questions
        WHERE QuestionTitle = 'Adventure'
        UNION ALL
        SELECT QuestionId, 'I''d rather have a properly planned trip'
        FROM Questions
        WHERE QuestionTitle = 'Adventure'
        UNION ALL
        SELECT QuestionId, 'Only if the destination and activity really interest me'
        FROM Questions
        WHERE QuestionTitle = 'Adventure';
    
    
        /* ---------------------------------------------------------
           11. Independence
           Relationships + Lifestyle + Values & Beliefs
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A lot of personal space is healthy'
        FROM Questions
        WHERE QuestionTitle = 'Independence'
        UNION ALL
        SELECT QuestionId, 'Some personal space, but frequent connection'
        FROM Questions
        WHERE QuestionTitle = 'Independence'
        UNION ALL
        SELECT QuestionId, 'Most things should be shared with a partner'
        FROM Questions
        WHERE QuestionTitle = 'Independence'
        UNION ALL
        SELECT QuestionId, 'It depends on the people involved'
        FROM Questions
        WHERE QuestionTitle = 'Independence';
    
    
        /* ---------------------------------------------------------
           12. Emotional Support
           Relationships + Love & Romance + Friendship
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Listen without immediately trying to fix anything'
        FROM Questions
        WHERE QuestionTitle = 'Emotional Support'
        UNION ALL
        SELECT QuestionId, 'Offer practical solutions'
        FROM Questions
        WHERE QuestionTitle = 'Emotional Support'
        UNION ALL
        SELECT QuestionId, 'Give them affection and reassurance'
        FROM Questions
        WHERE QuestionTitle = 'Emotional Support'
        UNION ALL
        SELECT QuestionId, 'Give them space until they are ready to talk'
        FROM Questions
        WHERE QuestionTitle = 'Emotional Support';
    
    
        /* ---------------------------------------------------------
           13. Money and Relationships
           Relationships + Lifestyle + Life & Goals
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Keep finances mostly separate'
        FROM Questions
        WHERE QuestionTitle = 'Money and Relationships'
        UNION ALL
        SELECT QuestionId, 'Combine finances and plan together'
        FROM Questions
        WHERE QuestionTitle = 'Money and Relationships'
        UNION ALL
        SELECT QuestionId, 'Have shared expenses but keep personal money'
        FROM Questions
        WHERE QuestionTitle = 'Money and Relationships'
        UNION ALL
        SELECT QuestionId, 'Create a financial system that works specifically for us'
        FROM Questions
        WHERE QuestionTitle = 'Money and Relationships';
    
    
        /* ---------------------------------------------------------
           14. Future Family
           Family + Love & Romance + Life & Goals
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'It is very important to me'
        FROM Questions
        WHERE QuestionTitle = 'Future Family'
        UNION ALL
        SELECT QuestionId, 'I would like it, but I am flexible'
        FROM Questions
        WHERE QuestionTitle = 'Future Family'
        UNION ALL
        SELECT QuestionId, 'I am unsure'
        FROM Questions
        WHERE QuestionTitle = 'Future Family'
        UNION ALL
        SELECT QuestionId, 'I do not see children as part of my future'
        FROM Questions
        WHERE QuestionTitle = 'Future Family';
    
    
        /* ---------------------------------------------------------
           15. Success
           Career & Ambition + Life & Goals + Values & Beliefs
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Building something I am proud of'
        FROM Questions
        WHERE QuestionTitle = 'Success'
        UNION ALL
        SELECT QuestionId, 'Having people I love around me'
        FROM Questions
        WHERE QuestionTitle = 'Success'
        UNION ALL
        SELECT QuestionId, 'Having financial freedom'
        FROM Questions
        WHERE QuestionTitle = 'Success'
        UNION ALL
        SELECT QuestionId, 'Becoming the best version of myself'
        FROM Questions
        WHERE QuestionTitle = 'Success';
    
    
        /* ---------------------------------------------------------
           16. Everyday Compatibility
           Lifestyle + Love & Romance + Relationships
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Going out for food and exploring somewhere'
        FROM Questions
        WHERE QuestionTitle = 'Everyday Compatibility'
        UNION ALL
        SELECT QuestionId, 'Staying home, cooking, and relaxing'
        FROM Questions
        WHERE QuestionTitle = 'Everyday Compatibility'
        UNION ALL
        SELECT QuestionId, 'Seeing friends and being social'
        FROM Questions
        WHERE QuestionTitle = 'Everyday Compatibility'
        UNION ALL
        SELECT QuestionId, 'Doing separate things while enjoying being together'
        FROM Questions
        WHERE QuestionTitle = 'Everyday Compatibility';
    
    
        /* ---------------------------------------------------------
           17. Trust
           Relationships + Love & Romance + Values & Beliefs
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Being honest even when it is difficult'
        FROM Questions
        WHERE QuestionTitle = 'Trust'
        UNION ALL
        SELECT QuestionId, 'Being consistent and keeping promises'
        FROM Questions
        WHERE QuestionTitle = 'Trust'
        UNION ALL
        SELECT QuestionId, 'Giving each other freedom and independence'
        FROM Questions
        WHERE QuestionTitle = 'Trust'
        UNION ALL
        SELECT QuestionId, 'Being emotionally open with each other'
        FROM Questions
        WHERE QuestionTitle = 'Trust';
    
    
        /* ---------------------------------------------------------
           18. Change
           Career & Ambition + Life & Goals + Relationships
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Support them even if the change is difficult'
        FROM Questions
        WHERE QuestionTitle = 'Change'
        UNION ALL
        SELECT QuestionId, 'Discuss how the change affects both of us'
        FROM Questions
        WHERE QuestionTitle = 'Change'
        UNION ALL
        SELECT QuestionId, 'Encourage them to follow what makes them happy'
        FROM Questions
        WHERE QuestionTitle = 'Change'
        UNION ALL
        SELECT QuestionId, 'Ask them to reconsider if it puts our future at risk'
        FROM Questions
        WHERE QuestionTitle = 'Change';
    
    
        /* ---------------------------------------------------------
           19. Romance
           Love & Romance + Relationships + Lifestyle
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Continuing to intentionally date each other'
        FROM Questions
        WHERE QuestionTitle = 'Romance'
        UNION ALL
        SELECT QuestionId, 'Small thoughtful gestures'
        FROM Questions
        WHERE QuestionTitle = 'Romance'
        UNION ALL
        SELECT QuestionId, 'Trying new experiences together'
        FROM Questions
        WHERE QuestionTitle = 'Romance'
        UNION ALL
        SELECT QuestionId, 'Keeping emotional and physical intimacy alive'
        FROM Questions
        WHERE QuestionTitle = 'Romance';
    
    
        /* ---------------------------------------------------------
           20. Meaningful Life
           Deep & Meaningful + Values & Beliefs + Life & Goals
           --------------------------------------------------------- */
    
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Loving and being loved by other people'
        FROM Questions
        WHERE QuestionTitle = 'Meaningful Life'
        UNION ALL
        SELECT QuestionId, 'Making a positive difference'
        FROM Questions
        WHERE QuestionTitle = 'Meaningful Life'
        UNION ALL
        SELECT QuestionId, 'Growing and discovering who I am'
        FROM Questions
        WHERE QuestionTitle = 'Meaningful Life'
        UNION ALL
        SELECT QuestionId, 'Experiencing everything life has to offer'
        FROM Questions
        WHERE QuestionTitle = 'Meaningful Life';
    
    
        /* =========================================================
           QUESTION → CATEGORY RELATIONSHIPS
           ========================================================= */
    
        INSERT INTO QuestionCategories
        (
            QuestionId,
            CategoryId
        )
        SELECT q.QuestionId, c.CategoryId
        FROM Questions q
        INNER JOIN Categories c
            ON
                /* 1 - Moving for Love */
                (q.QuestionTitle = 'Moving for Love'
                    AND c.CategoryName IN
                    (
                        'Love & Romance',
                        'Career & Ambition',
                        'Travel & Adventure',
                        'Life & Goals'
                    ))
    
                OR
    
                /* 2 - Handling Conflict */
                (q.QuestionTitle = 'Handling Conflict'
                    AND c.CategoryName IN
                    (
                        'Relationships',
                        'Love & Romance',
                        'Values & Beliefs'
                    ))
    
                OR
    
                /* 3 - Quality Time */
                (q.QuestionTitle = 'Quality Time'
                    AND c.CategoryName IN
                    (
                        'Relationships',
                        'Love & Romance',
                        'Lifestyle'
                    ))
    
                OR
    
                /* 4 - Financial Priorities */
                (q.QuestionTitle = 'Financial Priorities'
                    AND c.CategoryName IN
                    (
                        'Life & Goals',
                        'Career & Ambition',
                        'Lifestyle'
                    ))
    
                OR
    
                /* 5 - Ambition and Partnership */
                (q.QuestionTitle = 'Ambition and Partnership'
                    AND c.CategoryName IN
                    (
                        'Career & Ambition',
                        'Relationships',
                        'Love & Romance'
                    ))
    
                OR
    
                /* 6 - Social Life */
                (q.QuestionTitle = 'Social Life'
                    AND c.CategoryName IN
                    (
                        'Friendship',
                        'Lifestyle',
                        'Relationships'
                    ))
    
                OR
    
                /* 7 - Family Boundaries */
                (q.QuestionTitle = 'Family Boundaries'
                    AND c.CategoryName IN
                    (
                        'Family',
                        'Relationships',
                        'Values & Beliefs'
                    ))
    
                OR
    
                /* 8 - Affection */
                (q.QuestionTitle = 'Affection'
                    AND c.CategoryName IN
                    (
                        'Love & Romance',
                        'Relationships'
                    ))
    
                OR
    
                /* 9 - Life Direction */
                (q.QuestionTitle = 'Life Direction'
                    AND c.CategoryName IN
                    (
                        'Life & Goals',
                        'Career & Ambition',
                        'Love & Romance'
                    ))
    
                OR
    
                /* 10 - Adventure */
                (q.QuestionTitle = 'Adventure'
                    AND c.CategoryName IN
                    (
                        'Travel & Adventure',
                        'Lifestyle',
                        'Relationships'
                    ))
    
                OR
    
                /* 11 - Independence */
                (q.QuestionTitle = 'Independence'
                    AND c.CategoryName IN
                    (
                        'Relationships',
                        'Lifestyle',
                        'Values & Beliefs'
                    ))
    
                OR
    
                /* 12 - Emotional Support */
                (q.QuestionTitle = 'Emotional Support'
                    AND c.CategoryName IN
                    (
                        'Relationships',
                        'Love & Romance',
                        'Friendship'
                    ))
    
                OR
    
                /* 13 - Money and Relationships */
                (q.QuestionTitle = 'Money and Relationships'
                    AND c.CategoryName IN
                    (
                        'Relationships',
                        'Lifestyle',
                        'Life & Goals'
                    ))
    
                OR
    
                /* 14 - Future Family */
                (q.QuestionTitle = 'Future Family'
                    AND c.CategoryName IN
                    (
                        'Family',
                        'Love & Romance',
                        'Life & Goals'
                    ))
    
                OR
    
                /* 15 - Success */
                (q.QuestionTitle = 'Success'
                    AND c.CategoryName IN
                    (
                        'Career & Ambition',
                        'Life & Goals',
                        'Values & Beliefs'
                    ))
    
                OR
    
                /* 16 - Everyday Compatibility */
                (q.QuestionTitle = 'Everyday Compatibility'
                    AND c.CategoryName IN
                    (
                        'Lifestyle',
                        'Love & Romance',
                        'Relationships'
                    ))
    
                OR
    
                /* 17 - Trust */
                (q.QuestionTitle = 'Trust'
                    AND c.CategoryName IN
                    (
                        'Relationships',
                        'Love & Romance',
                        'Values & Beliefs'
                    ))
    
                OR
    
                /* 18 - Change */
                (q.QuestionTitle = 'Change'
                    AND c.CategoryName IN
                    (
                        'Career & Ambition',
                        'Life & Goals',
                        'Relationships'
                    ))
    
                OR
    
                /* 19 - Romance */
                (q.QuestionTitle = 'Romance'
                    AND c.CategoryName IN
                    (
                        'Love & Romance',
                        'Relationships',
                        'Lifestyle'
                    ))
    
                OR
    
                /* 20 - Meaningful Life */
                (q.QuestionTitle = 'Meaningful Life'
                    AND c.CategoryName IN
                    (
                        'Deep & Meaningful',
                        'Values & Beliefs',
                        'Life & Goals'
                    ));

    UPDATE Questions
    SET CreatedBy = N'SeedQuestionCatalog'
    WHERE QuestionTitle IN
    (
        N'Moving for Love',
        N'Handling Conflict',
        N'Quality Time',
        N'Financial Priorities',
        N'Ambition and Partnership',
        N'Social Life',
        N'Family Boundaries',
        N'Affection',
        N'Life Direction',
        N'Adventure',
        N'Independence',
        N'Emotional Support',
        N'Money and Relationships',
        N'Future Family',
        N'Success',
        N'Everyday Compatibility',
        N'Trust',
        N'Change',
        N'Romance',
        N'Meaningful Life'
    );
END
ELSE IF @ExistingQuestionCount2 <> 20
BEGIN
    THROW 51001, 'Question seed batch 2 is partially present. Complete or remove the partial batch before applying this migration.', 1;
END;

DECLARE @ExistingQuestionCount3 int =
(
    SELECT COUNT(DISTINCT QuestionTitle)
    FROM Questions
    WHERE QuestionTitle IN
    (
        N'Social Energy',
        N'Decision Making',
        N'Conflict',
        N'First Date',
        N'Friendship',
        N'Family',
        N'Life Goals',
        N'Values',
        N'Free Time',
        N'Travel',
        N'Career',
        N'Childhood',
        N'Food',
        N'Fun',
        N'Meaning'
    )
);

IF @ExistingQuestionCount3 = 0
BEGIN
    /* =========================================================
           QUESTIONS
           ========================================================= */
    
        INSERT INTO Questions
        (
            QuestionTitle,
            FullQuestion,
            IsActive,
            DateCreated,
            CreatedBy
        )
        VALUES
        (
            'Social Energy',
            'After a long week, what sounds most appealing to you?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Decision Making',
            'When making an important decision, what do you usually trust most?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Conflict',
            'When someone you care about upsets you, what are you most likely to do?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'First Date',
            'What would make a first date feel especially memorable?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Friendship',
            'What quality matters most to you in a close friend?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Family',
            'How do you usually show your family that you care about them?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Life Goals',
            'Which of these feels most important to you right now?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Values',
            'Which quality do you admire most in another person?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Free Time',
            'If you suddenly had an entire day with nothing planned, what would you most likely do?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Travel',
            'What kind of trip sounds most exciting to you?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Career',
            'What motivates you most at work?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Childhood',
            'Which childhood memory would you most enjoy reliving?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Food',
            'If you could only choose one type of food for a month, what would you pick?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Fun',
            'What kind of activity is most likely to make you laugh uncontrollably?',
            1,
            GETUTCDATE(),
            'Seed'
        ),
        (
            'Meaning',
            'What makes life feel meaningful to you?',
            1,
            GETUTCDATE(),
            'Seed'
        );
    
    
        /* =========================================================
           ANSWERS
           ========================================================= */
    
        -- Social Energy
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Going out and being around people'
        FROM Questions WHERE QuestionTitle = 'Social Energy'
        UNION ALL
        SELECT QuestionId, 'Spending time with a few close friends'
        FROM Questions WHERE QuestionTitle = 'Social Energy'
        UNION ALL
        SELECT QuestionId, 'Staying home and enjoying some quiet time'
        FROM Questions WHERE QuestionTitle = 'Social Energy'
        UNION ALL
        SELECT QuestionId, 'It depends entirely on my mood'
        FROM Questions WHERE QuestionTitle = 'Social Energy';
    
    
        -- Decision Making
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Logic and facts'
        FROM Questions WHERE QuestionTitle = 'Decision Making'
        UNION ALL
        SELECT QuestionId, 'My feelings'
        FROM Questions WHERE QuestionTitle = 'Decision Making'
        UNION ALL
        SELECT QuestionId, 'Advice from people I trust'
        FROM Questions WHERE QuestionTitle = 'Decision Making'
        UNION ALL
        SELECT QuestionId, 'A combination of everything'
        FROM Questions WHERE QuestionTitle = 'Decision Making';
    
    
        -- Conflict
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Talk about it immediately'
        FROM Questions WHERE QuestionTitle = 'Conflict'
        UNION ALL
        SELECT QuestionId, 'Take some time to cool down first'
        FROM Questions WHERE QuestionTitle = 'Conflict'
        UNION ALL
        SELECT QuestionId, 'Try to understand their perspective'
        FROM Questions WHERE QuestionTitle = 'Conflict'
        UNION ALL
        SELECT QuestionId, 'Avoid confrontation unless necessary'
        FROM Questions WHERE QuestionTitle = 'Conflict';
    
    
        -- First Date
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A deep conversation'
        FROM Questions WHERE QuestionTitle = 'First Date'
        UNION ALL
        SELECT QuestionId, 'Trying something neither of us has done before'
        FROM Questions WHERE QuestionTitle = 'First Date'
        UNION ALL
        SELECT QuestionId, 'A romantic dinner'
        FROM Questions WHERE QuestionTitle = 'First Date'
        UNION ALL
        SELECT QuestionId, 'Something spontaneous and unexpected'
        FROM Questions WHERE QuestionTitle = 'First Date';
    
    
        -- Friendship
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Loyalty'
        FROM Questions WHERE QuestionTitle = 'Friendship'
        UNION ALL
        SELECT QuestionId, 'Honesty'
        FROM Questions WHERE QuestionTitle = 'Friendship'
        UNION ALL
        SELECT QuestionId, 'Being able to have fun together'
        FROM Questions WHERE QuestionTitle = 'Friendship'
        UNION ALL
        SELECT QuestionId, 'Being there when things get difficult'
        FROM Questions WHERE QuestionTitle = 'Friendship';
    
    
        -- Family
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Spending quality time together'
        FROM Questions WHERE QuestionTitle = 'Family'
        UNION ALL
        SELECT QuestionId, 'Helping them when they need me'
        FROM Questions WHERE QuestionTitle = 'Family'
        UNION ALL
        SELECT QuestionId, 'Checking in regularly'
        FROM Questions WHERE QuestionTitle = 'Family'
        UNION ALL
        SELECT QuestionId, 'Showing affection openly'
        FROM Questions WHERE QuestionTitle = 'Family';
    
    
        -- Life Goals
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Building a successful career'
        FROM Questions WHERE QuestionTitle = 'Life Goals'
        UNION ALL
        SELECT QuestionId, 'Finding a meaningful relationship'
        FROM Questions WHERE QuestionTitle = 'Life Goals'
        UNION ALL
        SELECT QuestionId, 'Creating financial stability'
        FROM Questions WHERE QuestionTitle = 'Life Goals'
        UNION ALL
        SELECT QuestionId, 'Having freedom to enjoy life'
        FROM Questions WHERE QuestionTitle = 'Life Goals';
    
    
        -- Values
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Kindness'
        FROM Questions WHERE QuestionTitle = 'Values'
        UNION ALL
        SELECT QuestionId, 'Integrity'
        FROM Questions WHERE QuestionTitle = 'Values'
        UNION ALL
        SELECT QuestionId, 'Courage'
        FROM Questions WHERE QuestionTitle = 'Values'
        UNION ALL
        SELECT QuestionId, 'Empathy'
        FROM Questions WHERE QuestionTitle = 'Values';
    
    
        -- Free Time
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Go somewhere and explore'
        FROM Questions WHERE QuestionTitle = 'Free Time'
        UNION ALL
        SELECT QuestionId, 'Play games or watch something'
        FROM Questions WHERE QuestionTitle = 'Free Time'
        UNION ALL
        SELECT QuestionId, 'Spend time with friends'
        FROM Questions WHERE QuestionTitle = 'Free Time'
        UNION ALL
        SELECT QuestionId, 'Stay home and recharge'
        FROM Questions WHERE QuestionTitle = 'Free Time';
    
    
        -- Travel
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'A relaxing beach vacation'
        FROM Questions WHERE QuestionTitle = 'Travel'
        UNION ALL
        SELECT QuestionId, 'A spontaneous road trip'
        FROM Questions WHERE QuestionTitle = 'Travel'
        UNION ALL
        SELECT QuestionId, 'Exploring a new city'
        FROM Questions WHERE QuestionTitle = 'Travel'
        UNION ALL
        SELECT QuestionId, 'An outdoor adventure'
        FROM Questions WHERE QuestionTitle = 'Travel';
    
    
        -- Career
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Making a real impact'
        FROM Questions WHERE QuestionTitle = 'Career'
        UNION ALL
        SELECT QuestionId, 'Learning and improving'
        FROM Questions WHERE QuestionTitle = 'Career'
        UNION ALL
        SELECT QuestionId, 'Financial success'
        FROM Questions WHERE QuestionTitle = 'Career'
        UNION ALL
        SELECT QuestionId, 'Having freedom and flexibility'
        FROM Questions WHERE QuestionTitle = 'Career';
    
    
        -- Childhood
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Family celebrations'
        FROM Questions WHERE QuestionTitle = 'Childhood'
        UNION ALL
        SELECT QuestionId, 'Playing with friends'
        FROM Questions WHERE QuestionTitle = 'Childhood'
        UNION ALL
        SELECT QuestionId, 'School memories'
        FROM Questions WHERE QuestionTitle = 'Childhood'
        UNION ALL
        SELECT QuestionId, 'A special trip'
        FROM Questions WHERE QuestionTitle = 'Childhood';
    
    
        -- Food
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Asian food'
        FROM Questions WHERE QuestionTitle = 'Food'
        UNION ALL
        SELECT QuestionId, 'Italian food'
        FROM Questions WHERE QuestionTitle = 'Food'
        UNION ALL
        SELECT QuestionId, 'American comfort food'
        FROM Questions WHERE QuestionTitle = 'Food'
        UNION ALL
        SELECT QuestionId, 'Anything as long as dessert is included'
        FROM Questions WHERE QuestionTitle = 'Food';
    
    
        -- Fun
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'Ridiculous inside jokes'
        FROM Questions WHERE QuestionTitle = 'Fun'
        UNION ALL
        SELECT QuestionId, 'Competitive games'
        FROM Questions WHERE QuestionTitle = 'Fun'
        UNION ALL
        SELECT QuestionId, 'Random adventures'
        FROM Questions WHERE QuestionTitle = 'Fun'
        UNION ALL
        SELECT QuestionId, 'Watching something hilarious together'
        FROM Questions WHERE QuestionTitle = 'Fun';
    
    
        -- Meaning
        INSERT INTO Answers (QuestionId, AnswerText)
        SELECT QuestionId, 'The people I love'
        FROM Questions WHERE QuestionTitle = 'Meaning'
        UNION ALL
        SELECT QuestionId, 'Making a difference'
        FROM Questions WHERE QuestionTitle = 'Meaning'
        UNION ALL
        SELECT QuestionId, 'Growing into the person I want to become'
        FROM Questions WHERE QuestionTitle = 'Meaning'
        UNION ALL
        SELECT QuestionId, 'Experiencing as much of life as possible'
        FROM Questions WHERE QuestionTitle = 'Meaning';
    
    
        /* =========================================================
           QUESTION → CATEGORY LINKS
           ========================================================= */
    
        INSERT INTO QuestionCategories
        (
            QuestionId,
            CategoryId
        )
        SELECT
            q.QuestionId,
            c.CategoryId
        FROM Questions q
        INNER JOIN Categories c
            ON
                (q.QuestionTitle = 'Social Energy'
                    AND c.CategoryName = 'Personality')
                OR
                (q.QuestionTitle = 'Decision Making'
                    AND c.CategoryName = 'Personality')
                OR
                (q.QuestionTitle = 'Conflict'
                    AND c.CategoryName = 'Relationships')
                OR
                (q.QuestionTitle = 'First Date'
                    AND c.CategoryName = 'Love & Romance')
                OR
                (q.QuestionTitle = 'Friendship'
                    AND c.CategoryName = 'Friendship')
                OR
                (q.QuestionTitle = 'Family'
                    AND c.CategoryName = 'Family')
                OR
                (q.QuestionTitle = 'Life Goals'
                    AND c.CategoryName = 'Life & Goals')
                OR
                (q.QuestionTitle = 'Values'
                    AND c.CategoryName = 'Values & Beliefs')
                OR
                (q.QuestionTitle = 'Free Time'
                    AND c.CategoryName = 'Lifestyle')
                OR
                (q.QuestionTitle = 'Travel'
                    AND c.CategoryName = 'Travel & Adventure')
                OR
                (q.QuestionTitle = 'Career'
                    AND c.CategoryName = 'Career & Ambition')
                OR
                (q.QuestionTitle = 'Childhood'
                    AND c.CategoryName = 'Childhood & Memories')
                OR
                (q.QuestionTitle = 'Food'
                    AND c.CategoryName = 'Food & Drink')
                OR
                (q.QuestionTitle = 'Fun'
                    AND c.CategoryName = 'Fun & Random')
                OR
                (q.QuestionTitle = 'Meaning'
                    AND c.CategoryName = 'Deep & Meaningful');
    
    
        /* =========================================================
           COMMIT
           ========================================================= */

    UPDATE Questions
    SET CreatedBy = N'SeedQuestionCatalog'
    WHERE QuestionTitle IN
    (
        N'Social Energy',
        N'Decision Making',
        N'Conflict',
        N'First Date',
        N'Friendship',
        N'Family',
        N'Life Goals',
        N'Values',
        N'Free Time',
        N'Travel',
        N'Career',
        N'Childhood',
        N'Food',
        N'Fun',
        N'Meaning'
    );
END
ELSE IF @ExistingQuestionCount3 <> 15
BEGIN
    THROW 51002, 'Question seed batch 3 is partially present. Complete or remove the partial batch before applying this migration.', 1;
END;
""");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
"""
IF EXISTS
(
    SELECT 1
    FROM UserAnswers ua
    INNER JOIN Answers a ON a.AnswerId = ua.AnswerId
    INNER JOIN Questions q ON q.QuestionId = a.QuestionId
    WHERE q.CreatedBy = N'SeedQuestionCatalog'
)
BEGIN
    THROW 51010, 'Cannot remove seeded questions after users have answered them.', 1;
END;

DELETE FROM Questions
WHERE CreatedBy = N'SeedQuestionCatalog';

DELETE c
FROM Categories c
WHERE c.CreatedBy = N'SeedQuestionCatalog'
  AND NOT EXISTS
  (
      SELECT 1
      FROM QuestionCategories qc
      WHERE qc.CategoryId = c.CategoryId
  );
""");
    }
}
