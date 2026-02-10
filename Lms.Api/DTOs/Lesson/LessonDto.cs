namespace Lms.Api.DTOs.Lesson;

public class LessonDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public string? VideoUrl { get; init; }
    public string? DocumentUrl { get; init; }
    public int OrderIndex { get; init; }
    public DateTime CreatedAt { get; init; }
}
