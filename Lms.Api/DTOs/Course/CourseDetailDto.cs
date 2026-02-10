using Lms.Api.DTOs.Lesson;

namespace Lms.Api.DTOs.Course;

public class CourseDetailDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public string CreatorName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<LessonDto> Lessons { get; init; } = new();
}
