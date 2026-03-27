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
        try
        {
            // Force new IDs
            quiz.Id = 0;
            foreach (var question in quiz.Questions)
            {
                question.Id = 0;
                question.QuizId = 0;
                question.Quiz = null; // Clear navigation property

                foreach (var answer in question.Answers)
                {
                    answer.Id = 0;
                    answer.QuestionId = 0;
                    answer.Question = null; // Clear navigation property
                }
            }

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            // Return only the ID, not the full object
            return CreatedAtAction(nameof(GetQuiz), new { id = quiz.Id }, new { id = quiz.Id });
        }
        catch (Exception ex)
        {
            return BadRequest(new
            {
                message = ex.Message,
                innerException = ex.InnerException?.Message
            });
        }
    }

    // PUT: api/quiz/admin/5
    [HttpPut("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateQuiz(int id, Quiz quiz)
    {
        try
        {
            var existingQuiz = await _context.Quizzes
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (existingQuiz == null)
                return NotFound();

            // Update quiz properties
            existingQuiz.CategoryName = quiz.CategoryName;
            existingQuiz.Title = quiz.Title;
            existingQuiz.IsActive = quiz.IsActive;

            // Track which question IDs are in the update
            var incomingQuestionIds = quiz.Questions
                .Where(q => q.Id > 0)
                .Select(q => q.Id)
                .ToList();

            // Delete questions that are no longer in the quiz
            var questionsToDelete = existingQuiz.Questions
                .Where(q => !incomingQuestionIds.Contains(q.Id))
                .ToList();
            _context.Questions.RemoveRange(questionsToDelete);

            // Process each question
            foreach (var incomingQuestion in quiz.Questions)
            {
                if (incomingQuestion.Id > 0)
                {
                    // UPDATE existing question
                    var existingQuestion = existingQuiz.Questions
                        .FirstOrDefault(q => q.Id == incomingQuestion.Id);
                    
                    if (existingQuestion != null)
                    {
                        existingQuestion.QuestionText = incomingQuestion.QuestionText;
                        existingQuestion.MediaUrl = incomingQuestion.MediaUrl;
                        existingQuestion.MediaType = incomingQuestion.MediaType;
                        existingQuestion.OrderIndex = incomingQuestion.OrderIndex;

                        // Track which answer IDs are in the update
                        var incomingAnswerIds = incomingQuestion.Answers
                            .Where(a => a.Id > 0)
                            .Select(a => a.Id)
                            .ToList();

                        // Delete answers no longer in the question
                        var answersToDelete = existingQuestion.Answers
                            .Where(a => !incomingAnswerIds.Contains(a.Id))
                            .ToList();
                        _context.Answers.RemoveRange(answersToDelete);

                        // Process each answer
                        foreach (var incomingAnswer in incomingQuestion.Answers)
                        {
                            if (incomingAnswer.Id > 0)
                            {
                                // UPDATE existing answer
                                var existingAnswer = existingQuestion.Answers
                                    .FirstOrDefault(a => a.Id == incomingAnswer.Id);
                                
                                if (existingAnswer != null)
                                {
                                    existingAnswer.AnswerText = incomingAnswer.AnswerText;
                                    existingAnswer.IsCorrect = incomingAnswer.IsCorrect;
                                    existingAnswer.OrderIndex = incomingAnswer.OrderIndex;
                                }
                            }
                            else
                            {
                                // CREATE new answer
                                existingQuestion.Answers.Add(new Answer
                                {
                                    AnswerText = incomingAnswer.AnswerText,
                                    IsCorrect = incomingAnswer.IsCorrect,
                                    OrderIndex = incomingAnswer.OrderIndex
                                });
                            }
                        }
                    }
                }
                else
                {
                    // CREATE new question
                    var newQuestion = new Question
                    {
                        QuizId = id,
                        QuestionText = incomingQuestion.QuestionText,
                        MediaUrl = incomingQuestion.MediaUrl,
                        MediaType = incomingQuestion.MediaType,
                        OrderIndex = incomingQuestion.OrderIndex,
                        Answers = incomingQuestion.Answers.Select(a => new Answer
                        {
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect,
                            OrderIndex = a.OrderIndex
                        }).ToList()
                    };
                    existingQuiz.Questions.Add(newQuestion);
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                message = ex.Message, 
                innerException = ex.InnerException?.Message 
            });
        }
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