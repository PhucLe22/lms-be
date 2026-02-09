using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Course;

public class UpdateCourseDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}
