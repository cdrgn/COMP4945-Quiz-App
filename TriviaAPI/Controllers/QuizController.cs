using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TriviaAPI.Data;
using TriviaAPI.Models;

namespace TriviaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly AppDbContext _context;

    public QuizController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/quiz/categories
    [HttpGet("categories")]
    public async Task<ActionResult<List<object>>> GetCategories()
    {
        var categories = await _context.Quizzes
            .Where(q => q.IsActive)
            .Select(q => new
            {
                q.Id,
                q.CategoryName,
                q.Title,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    // GET: api/quiz/5
    [HttpGet("{id}")]
    public async Task<ActionResult<QuizDto>> GetQuiz(int id)
    {
        var quiz = await _context.Quizzes
            .Where(q => q.Id == id)
            .Select(q => new QuizDto
            {
                Id = q.Id,
                CategoryName = q.CategoryName,
                Title = q.Title,
                Questions = q.Questions
                    .OrderBy(quest => quest.OrderIndex)
                    .Select(quest => new QuestionDto
                    {
                        Id = quest.Id,
                        QuestionText = quest.QuestionText,
                        MediaUrl = quest.MediaUrl,
                        MediaType = quest.MediaType,
                        Answers = quest.Answers
                            .OrderBy(a => a.OrderIndex)
                            .Select(a => new AnswerDto
                            {
                                Id = a.Id,
                                AnswerText = a.AnswerText,
                                IsCorrect = a.IsCorrect
                            }).ToList()
                    }).ToList()
            })
            .FirstOrDefaultAsync();

        if (quiz == null)
        {
            return NotFound();
        }

        return Ok(quiz);
    }

    // ADMIN ONLY ENDPOINTS

    // GET: api/quiz/admin/all
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<object>>> GetAllQuizzes()
    {
        var quizzes = await _context.Quizzes
            .Select(q => new
            {
                q.Id,
                q.CategoryName,
                q.Title,
                q.IsActive,
                QuestionCount = q.Questions.Count
            })
            .ToListAsync();

        return Ok(quizzes);
    }

    // POST: api/quiz/admin
    [HttpPost("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Quiz>> CreateQuiz(Quiz quiz)
    {
        _context.Quizzes.Add(quiz);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, quiz);
    }

    // PUT: api/quiz/admin/5
    [HttpPut("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuiz(int id, Quiz quiz)
    {
        if (id != quiz.Id)
        {
            return BadRequest();
        }

        // Delete existing questions and answers
        var existingQuiz = await _context.Quizzes
            .Include(q => q.Questions)
            .ThenInclude(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (existingQuiz == null)
        {
            return NotFound();
        }

        // Update basic properties
        existingQuiz.CategoryName = quiz.CategoryName;
        existingQuiz.Title = quiz.Title;
        existingQuiz.IsActive = quiz.IsActive;

        // Remove old questions (cascade will delete answers)
        _context.Questions.RemoveRange(existingQuiz.Questions);

        // Add new questions
        existingQuiz.Questions = quiz.Questions;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Quizzes.AnyAsync(q => q.Id == id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    // DELETE: api/quiz/admin/5
    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteQuiz(int id)
    {
        var quiz = await _context.Quizzes.FindAsync(id);
        if (quiz == null)
        {
            return NotFound();
        }

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}