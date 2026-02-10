namespace Lms.Api.DTOs.Quiz;

public class QuizDto
{
    public Guid Id { get; init; }
    public string Question { get; init; } = string.Empty;
    public string OptionA { get; init; } = string.Empty;
    public string OptionB { get; init; } = string.Empty;
    public string OptionC { get; init; } = string.Empty;
    public string OptionD { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
}
