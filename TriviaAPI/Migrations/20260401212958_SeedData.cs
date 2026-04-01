using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TriviaAPI.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ========================================
            // SEED USERS
            // ========================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""Role"", ""CreatedAt"")
                VALUES ('admin@hotmail.com', '$2a$11$K5K5K5K5K5K5K5K5K5K5K.eZQYzJQYzJQYzJQYzJQYzJQYzJQ', 'Admin', TO_TIMESTAMP('2026-04-01 00:00:00', 'YYYY-MM-DD HH24:MI:SS'))
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Users"" (""Email"", ""PasswordHash"", ""Role"", ""CreatedAt"")
                VALUES ('user@hotmail.com', '$2a$11$L6L6L6L6L6L6L6L6L6L6L.fAZRzKRzKRzKRzKRzKRzKRzKRz', 'User', TO_TIMESTAMP('2026-04-01 00:00:00', 'YYYY-MM-DD HH24:MI:SS'))
            ");

            // ========================================
            // SEED QUIZZES
            // ========================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Quizzes"" (""CategoryName"", ""Title"", ""IsActive"", ""CreatedAt"")
                VALUES ('1960s Music', 'Classic Hits from the 60s', 1, TO_TIMESTAMP('2026-04-01 00:00:00', 'YYYY-MM-DD HH24:MI:SS'))
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Quizzes"" (""CategoryName"", ""Title"", ""IsActive"", ""CreatedAt"")
                VALUES ('1970s Movies', 'Iconic Films of the 70s', 1, TO_TIMESTAMP('2026-04-01 00:00:00', 'YYYY-MM-DD HH24:MI:SS'))
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Quizzes"" (""CategoryName"", ""Title"", ""IsActive"", ""CreatedAt"")
                VALUES ('1950s History', 'Historical Events of the 50s', 1, TO_TIMESTAMP('2026-04-01 00:00:00', 'YYYY-MM-DD HH24:MI:SS'))
            ");

            // ========================================
            // QUIZ 1: 1960s Music - QUESTIONS
            // ========================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""MediaType"", ""MediaUrl"", ""OrderIndex"")
                VALUES (1, 'Who sang ''Imagine''?', 'audio', '/media/imagine.mp3', 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""MediaType"", ""MediaUrl"", ""OrderIndex"")
                VALUES (1, 'Which band is shown in this photo?', 'image', '/media/beatles.jpg', 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""OrderIndex"")
                VALUES (1, 'Who sang ''Respect''?', 3)
            ");

            // QUIZ 1, QUESTION 1 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (1, 'John Lennon', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (1, 'Paul McCartney', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (1, 'Bob Dylan', 0, 3)
            ");

            // QUIZ 1, QUESTION 2 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (2, 'The Beatles', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (2, 'The Rolling Stones', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (2, 'The Who', 0, 3)
            ");

            // QUIZ 1, QUESTION 3 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (3, 'Aretha Franklin', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (3, 'Diana Ross', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (3, 'Etta James', 0, 3)
            ");

            // ========================================
            // QUIZ 2: 1970s Movies - QUESTIONS
            // ========================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""MediaType"", ""MediaUrl"", ""OrderIndex"")
                VALUES (2, 'Name this classic movie scene', 'video', '/media/godfather.mp4', 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""MediaType"", ""MediaUrl"", ""OrderIndex"")
                VALUES (2, 'Who is this legendary actor?', 'image', '/media/deniro.jpg', 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""OrderIndex"")
                VALUES (2, 'Complete the quote: ''You talkin'' to me?''', 3)
            ");

            // QUIZ 2, QUESTION 1 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (4, 'The Godfather', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (4, 'Scarface', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (4, 'Goodfellas', 0, 3)
            ");

            // QUIZ 2, QUESTION 2 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (5, 'Robert De Niro', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (5, 'Al Pacino', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (5, 'Marlon Brando', 0, 3)
            ");

            // QUIZ 2, QUESTION 3 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (6, 'Taxi Driver', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (6, 'Raging Bull', 0, 2)
            "); 
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (6, 'The Deer Hunter', 0, 3)
            "); 

            // ========================================
            // QUIZ 3: 1950s History - QUESTIONS
            // ========================================
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""MediaType"", ""MediaUrl"", ""OrderIndex"")
                VALUES (3, 'What year did this event occur?', 'image', '/media/1950s-event.jpg', 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""OrderIndex"")
                VALUES (3, 'Who was US President in 1953?', 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Questions"" (""QuizId"", ""QuestionText"", ""MediaType"", ""MediaUrl"", ""OrderIndex"")
                VALUES (3, 'Identify this famous speech', 'audio', '/media/speech.mp3', 3)
            ");

            // QUIZ 3, QUESTION 1 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (7, '1955', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (7, '1952', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (7, '1958', 0, 3)
            ");

            // QUIZ 3, QUESTION 2 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (8, 'Dwight D. Eisenhower', 1, 1)
            "); 
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (8, 'Harry S. Truman', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (8, 'John F. Kennedy', 0, 3)
            ");

            // QUIZ 3, QUESTION 3 - ANSWERS
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (9, 'Eisenhower''s Farewell', 1, 1)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (9, 'Churchill''s Iron Curtain', 0, 2)
            ");
            
            migrationBuilder.Sql(@"
                INSERT INTO ""Answers"" (""QuestionId"", ""AnswerText"", ""IsCorrect"", ""OrderIndex"")
                VALUES (9, 'Truman Doctrine', 0, 3)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Delete in reverse order (respect foreign keys)
            migrationBuilder.Sql("DELETE FROM \"Answers\"");
            migrationBuilder.Sql("DELETE FROM \"Questions\"");
            migrationBuilder.Sql("DELETE FROM \"Quizzes\"");
            migrationBuilder.Sql("DELETE FROM \"Users\"");
        }
    }
}
