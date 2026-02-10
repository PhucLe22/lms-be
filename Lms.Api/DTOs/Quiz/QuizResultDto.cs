namespace Lms.Api.DTOs.Quiz;

public class QuizResultDto
{
    public Guid Id { get; init; }
    public Guid LessonId { get; init; }
    public int TotalQuestions { get; init; }
    public int CorrectAnswers { get; init; }
    public int Score { get; init; }
    public DateTime CompletedAt { get; init; }
}
