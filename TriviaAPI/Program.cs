using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using TriviaAPI.Data;
using TriviaAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-key-min-32-characters-long!";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Auto-apply migrations and seed from JSON
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    
    // Seed from JSON if database is empty
    if (!db.Quizzes.Any())
    {
        var seedDate = new DateTime(2026, 4, 1, 0, 0, 0, DateTimeKind.Utc);
        
        // Seed users first
        var adminHash = "$2a$11$K5K5K5K5K5K5K5K5K5K5K.eZQYzJQYzJQYzJQYzJQYzJQYzJQ";
        var userHash = "$2a$11$L6L6L6L6L6L6L6L6L6L6L.fAZRzKRzKRzKRzKRzKRzKRzKRz";
        
        db.Users.AddRange(
            new User { Email = "admin@hotmail.com", PasswordHash = adminHash, Role = "Admin", CreatedAt = seedDate },
            new User { Email = "user@hotmail.com", PasswordHash = userHash, Role = "User", CreatedAt = seedDate }
        );
        db.SaveChanges();
        
        // Load quizzes from JSON
        var jsonPath = Path.Combine(AppContext.BaseDirectory, "SeedData", "quizzes.json");
        var jsonText = File.ReadAllText(jsonPath);
        var quizData = JsonSerializer.Deserialize<List<QuizSeedData>>(jsonText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        
        if (quizData != null)
        {
            foreach (var quizSeed in quizData)
            {
                var quiz = new Quiz
                {
                    CategoryName = quizSeed.CategoryName,
                    Title = quizSeed.Title,
                    IsActive = true,
                    CreatedAt = seedDate
                };
                
                db.Quizzes.Add(quiz);
                db.SaveChanges(); // Save to get auto-generated Quiz ID
                
                foreach (var questionSeed in quizSeed.Questions)
                {
                    var question = new Question
                    {
                        QuizId = quiz.Id, // Use auto-generated ID
                        QuestionText = questionSeed.QuestionText,
                        MediaType = questionSeed.MediaType,
                        MediaUrl = questionSeed.MediaUrl,
                        OrderIndex = questionSeed.OrderIndex
                    };
                    
                    db.Questions.Add(question);
                    db.SaveChanges(); // Save to get auto-generated Question ID
                    
                    foreach (var answerSeed in questionSeed.Answers)
                    {
                        var answer = new Answer
                        {
                            QuestionId = question.Id, // Use auto-generated ID
                            AnswerText = answerSeed.AnswerText,
                            IsCorrect = answerSeed.IsCorrect,
                            OrderIndex = answerSeed.OrderIndex
                        };
                        
                        db.Answers.Add(answer);
                    }
                }
                
                db.SaveChanges(); // Save all answers for this quiz
            }
        }
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Serve static files (media)
app.UseStaticFiles();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();