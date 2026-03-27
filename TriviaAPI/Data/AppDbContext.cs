using Microsoft.EntityFrameworkCore;
using TriviaAPI.Models;

namespace TriviaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Answer> Answers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Quiz)
            .WithMany(qz => qz.Questions)
            .HasForeignKey(q => q.QuizId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed admin user (password: "admin123")
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Email = "admin@hotmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Role = "Admin"
            },
            new User
            {
                Id = 2,
                Email = "user@hotmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
                Role = "User"
            }
        );

        // Seed 3 quizzes
        modelBuilder.Entity<Quiz>().HasData(
            new Quiz { Id = 1, CategoryName = "1960s Music", Title = "Classic Hits from the 60s" },
            new Quiz { Id = 2, CategoryName = "1970s Movies", Title = "Iconic Films of the 70s" },
            new Quiz { Id = 3, CategoryName = "1950s History", Title = "Historical Events of the 50s" }
        );

        // Quiz 1: 1960s Music (3 questions)
        modelBuilder.Entity<Question>().HasData(
            new Question { Id = 1, QuizId = 1, QuestionText = "Who sang 'Imagine'?", MediaType = "audio", MediaUrl = "/media/imagine.mp3", OrderIndex = 1 },
            new Question { Id = 2, QuizId = 1, QuestionText = "Which band is shown in this photo?", MediaType = "image", MediaUrl = "/media/beatles.jpg", OrderIndex = 2 },
            new Question { Id = 3, QuizId = 1, QuestionText = "Who sang 'Respect'?", MediaType = null, MediaUrl = null, OrderIndex = 3 }
        );

        modelBuilder.Entity<Answer>().HasData(
            // Q1 answers
            new Answer { Id = 1, QuestionId = 1, AnswerText = "John Lennon", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 2, QuestionId = 1, AnswerText = "Paul McCartney", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 3, QuestionId = 1, AnswerText = "Bob Dylan", IsCorrect = false, OrderIndex = 3 },
            // Q2 answers
            new Answer { Id = 4, QuestionId = 2, AnswerText = "The Beatles", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 5, QuestionId = 2, AnswerText = "The Rolling Stones", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 6, QuestionId = 2, AnswerText = "The Who", IsCorrect = false, OrderIndex = 3 },
            // Q3 answers
            new Answer { Id = 7, QuestionId = 3, AnswerText = "Aretha Franklin", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 8, QuestionId = 3, AnswerText = "Diana Ross", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 9, QuestionId = 3, AnswerText = "Etta James", IsCorrect = false, OrderIndex = 3 }
        );

        // Quiz 2: 1970s Movies (3 questions)
        modelBuilder.Entity<Question>().HasData(
            new Question { Id = 4, QuizId = 2, QuestionText = "Name this classic movie scene", MediaType = "video", MediaUrl = "/media/godfather.mp4", OrderIndex = 1 },
            new Question { Id = 5, QuizId = 2, QuestionText = "Who is this legendary actor?", MediaType = "image", MediaUrl = "/media/deniro.jpg", OrderIndex = 2 },
            new Question { Id = 6, QuizId = 2, QuestionText = "Complete the quote: 'You talkin' to me?'", MediaType = null, MediaUrl = null, OrderIndex = 3 }
        );

        modelBuilder.Entity<Answer>().HasData(
            // Q4 answers
            new Answer { Id = 10, QuestionId = 4, AnswerText = "The Godfather", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 11, QuestionId = 4, AnswerText = "Scarface", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 12, QuestionId = 4, AnswerText = "Goodfellas", IsCorrect = false, OrderIndex = 3 },
            // Q5 answers
            new Answer { Id = 13, QuestionId = 5, AnswerText = "Robert De Niro", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 14, QuestionId = 5, AnswerText = "Al Pacino", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 15, QuestionId = 5, AnswerText = "Marlon Brando", IsCorrect = false, OrderIndex = 3 },
            // Q6 answers
            new Answer { Id = 16, QuestionId = 6, AnswerText = "Taxi Driver", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 17, QuestionId = 6, AnswerText = "Raging Bull", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 18, QuestionId = 6, AnswerText = "The Deer Hunter", IsCorrect = false, OrderIndex = 3 }
        );

        // Quiz 3: 1950s History (3 questions)
        modelBuilder.Entity<Question>().HasData(
            new Question { Id = 7, QuizId = 3, QuestionText = "What year did this event occur?", MediaType = "image", MediaUrl = "/media/1950s-event.jpg", OrderIndex = 1 },
            new Question { Id = 8, QuizId = 3, QuestionText = "Who was US President in 1953?", MediaType = null, MediaUrl = null, OrderIndex = 2 },
            new Question { Id = 9, QuizId = 3, QuestionText = "Identify this famous speech", MediaType = "audio", MediaUrl = "/media/speech.mp3", OrderIndex = 3 }
        );

        modelBuilder.Entity<Answer>().HasData(
            // Q7 answers
            new Answer { Id = 19, QuestionId = 7, AnswerText = "1955", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 20, QuestionId = 7, AnswerText = "1952", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 21, QuestionId = 7, AnswerText = "1958", IsCorrect = false, OrderIndex = 3 },
            // Q8 answers
            new Answer { Id = 22, QuestionId = 8, AnswerText = "Dwight D. Eisenhower", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 23, QuestionId = 8, AnswerText = "Harry S. Truman", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 24, QuestionId = 8, AnswerText = "John F. Kennedy", IsCorrect = false, OrderIndex = 3 },
            // Q9 answers
            new Answer { Id = 25, QuestionId = 9, AnswerText = "Eisenhower's Farewell", IsCorrect = true, OrderIndex = 1 },
            new Answer { Id = 26, QuestionId = 9, AnswerText = "Churchill's Iron Curtain", IsCorrect = false, OrderIndex = 2 },
            new Answer { Id = 27, QuestionId = 9, AnswerText = "Truman Doctrine", IsCorrect = false, OrderIndex = 3 }
        );
    }
}