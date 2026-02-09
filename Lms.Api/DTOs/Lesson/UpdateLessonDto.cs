using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Lesson;

public class UpdateLessonDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [Required]
    public string Content { get; init; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int OrderIndex { get; init; }
}
