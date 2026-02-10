namespace Lms.Api.DTOs.LessonProgress;

public class LessonProgressDto
{
    public Guid LessonId { get; init; }
    public string LessonTitle { get; init; } = string.Empty;
    public int OrderIndex { get; init; }
    public int VideoWatchPercent { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
}
