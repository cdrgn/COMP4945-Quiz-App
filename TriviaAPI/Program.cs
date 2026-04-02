using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using TriviaAPI.Data;
using TriviaAPI.Hubs;    // ← NEW: for QuizHub
using TriviaAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── NEW: Register SignalR ───────────────────────────────────────────
builder.Services.AddSignalR();

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

        // ── NEW: Allow JWT token via query string for WebSocket connections ──
        // SignalR can't send Authorization headers during the WebSocket handshake,
        // so the token is passed as ?access_token=... in the connection URL.
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                // Only read from query string for hub endpoints
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

// CORS — updated to allow credentials (required for SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.SetIsOriginAllowed(origin => origin.StartsWith("http://localhost"))
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
        var adminHash = BCrypt.Net.BCrypt.HashPassword("password");
        var userHash = BCrypt.Net.BCrypt.HashPassword("password");
        
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
                db.SaveChanges();
                
                foreach (var questionSeed in quizSeed.Questions)
                {
                    var question = new Question
                    {
                        QuizId = quiz.Id,
                        QuestionText = questionSeed.QuestionText,
                        MediaType = questionSeed.MediaType,
                        MediaUrl = questionSeed.MediaUrl,
                        OrderIndex = questionSeed.OrderIndex
                    };
                    
                    db.Questions.Add(question);
                    db.SaveChanges();
                    
                    foreach (var answerSeed in questionSeed.Answers)
                    {
                        var answer = new Answer
                        {
                            QuestionId = question.Id,
                            AnswerText = answerSeed.AnswerText,
                            IsCorrect = answerSeed.IsCorrect,
                            OrderIndex = answerSeed.OrderIndex
                        };
                        
                        db.Answers.Add(answer);
                    }
                }
                
                db.SaveChanges();
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

// ── NEW: Map the SignalR hub endpoint ───────────────────────────────
app.MapHub<QuizHub>("/hubs/quiz");

app.Run();