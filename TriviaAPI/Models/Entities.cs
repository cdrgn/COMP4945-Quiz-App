using System.ComponentModel.DataAnnotations;

namespace TriviaAPI.Models;

public class User
{
    public int Id { get; set; }
	[MaxLength(100)]
    public string Email { get; set; } = string.Empty;
	[MaxLength(100)]
    public string PasswordHash { get; set; } = string.Empty;
	[MaxLength(20)]
    public string Role { get; set; } = "User"; // "Admin" or "User"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class Quiz
{
    public int Id { get; set; }
	[MaxLength(50)]
    public string CategoryName { get; set; } = string.Empty;
	[MaxLength(100)]
	public string Title { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<Question> Questions { get; set; } = new();
}

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
	[MaxLength(200)]
    public string QuestionText { get; set; } = string.Empty;
	[MaxLength(255)]
    public string? MediaUrl { get; set; }
	[MaxLength(10)]
    public string? MediaType { get; set; } // "image", "audio", "video", null
    public int OrderIndex { get; set; }
    
    public Quiz? Quiz { get; set; } = null;
    public List<Answer> Answers { get; set; } = new();
}

public class Answer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
	[MaxLength(200)]
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
    
    public Question? Question { get; set; } = null;
}

// DTOs for API requests/responses
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class QuizDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<QuestionDto> Questions { get; set; } = new();
}

public class QuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public string? MediaUrl { get; set; }
    public string? MediaType { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}

public class AnswerDto
{
    public int Id { get; set; }
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}