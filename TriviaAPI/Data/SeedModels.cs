namespace TriviaAPI.Data;

public class QuizSeedData
{
    public string CategoryName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<QuestionSeedData> Questions { get; set; } = new();
}

public class QuestionSeedData
{
    public string QuestionText { get; set; } = string.Empty;
    public string? MediaType { get; set; }
    public string? MediaUrl { get; set; }
    public int OrderIndex { get; set; }
    public List<AnswerSeedData> Answers { get; set; } = new();
}

public class AnswerSeedData
{
    public string AnswerText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}