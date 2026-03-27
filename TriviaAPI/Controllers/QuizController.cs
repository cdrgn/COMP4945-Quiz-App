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
            .Include(q => q.Questions.OrderBy(q => q.OrderIndex))
            .ThenInclude(q => q.Answers.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz == null)
        {
            return NotFound();
        }

        var quizDto = new QuizDto
        {
            Id = quiz.Id,
            CategoryName = quiz.CategoryName,
            Title = quiz.Title,
            Questions = quiz.Questions.Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                MediaUrl = q.MediaUrl,
                MediaType = q.MediaType,
                Answers = q.Answers.Select(a => new AnswerDto
                {
                    Id = a.Id,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect
                }).ToList()
            }).ToList()
        };

        return Ok(quizDto);
    }

    // ADMIN ONLY ENDPOINTS

    // GET: api/quiz/admin/all
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Quiz>>> GetAllQuizzes()
    {
        var quizzes = await _context.Quizzes
            .Include(q => q.Questions)
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

        _context.Entry(quiz).State = EntityState.Modified;

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