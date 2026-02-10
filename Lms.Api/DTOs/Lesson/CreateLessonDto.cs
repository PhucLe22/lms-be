using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Lesson;

public class CreateLessonDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Content { get; init; } = string.Empty;

    [MaxLength(500)]
    public string? VideoUrl { get; init; }

    [MaxLength(500)]
    public string? DocumentUrl { get; init; }

    [Range(0, int.MaxValue)]
    public int OrderIndex { get; init; }
}
