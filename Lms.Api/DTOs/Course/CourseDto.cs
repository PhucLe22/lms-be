namespace Lms.Api.DTOs.Course;

public class CourseDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public string CreatorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int LessonCount { get; init; }
    public int EnrollmentCount { get; init; }
}
