namespace Lms.Api.DTOs.Lesson;

public class LessonDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public DateTime CreatedAt { get; init; }
}
