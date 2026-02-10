using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Practice;

public class CreatePracticeTaskDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required, MaxLength(5000)]
    public string Description { get; init; } = string.Empty;
}
