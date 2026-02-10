using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Quiz;

public class CreateQuizDto
{
    [Required, MaxLength(1000)]
    public string Question { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionA { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionB { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionC { get; init; } = string.Empty;

    [Required, MaxLength(500)]
    public string OptionD { get; init; } = string.Empty;

    [Required]
    [RegularExpression("^(A|B|C|D)$", ErrorMessage = "CorrectAnswer must be 'A', 'B', 'C', or 'D'.")]
    public string CorrectAnswer { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int OrderIndex { get; init; }
}
