using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Course;

public class UpdateCourseDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;

    [RegularExpression("^(Beginner|Intermediate|Advanced)$", ErrorMessage = "Level must be 'Beginner', 'Intermediate', or 'Advanced'.")]
    public string Level { get; init; } = "Beginner";
}
