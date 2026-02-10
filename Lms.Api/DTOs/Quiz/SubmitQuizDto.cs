using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Quiz;

public class SubmitQuizDto
{
    [Required]
    public List<QuizAnswerDto> Answers { get; init; } = new();
}

public class QuizAnswerDto
{
    [Required]
    public Guid QuizId { get; init; }

    [Required]
    [RegularExpression("^(A|B|C|D)$", ErrorMessage = "Answer must be 'A', 'B', 'C', or 'D'.")]
    public string Answer { get; init; } = string.Empty;
}
