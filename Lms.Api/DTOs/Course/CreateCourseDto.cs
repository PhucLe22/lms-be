using System.ComponentModel.DataAnnotations;

namespace Lms.Api.DTOs.Course;

public class CreateCourseDto
{
    [Required, MaxLength(300)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; init; } = string.Empty;
}
