namespace Lms.Api.DTOs.Practice;

public class PracticeTaskDto
{
    public Guid Id { get; init; }
    public Guid LessonId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
