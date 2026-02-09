namespace Lms.Api.DTOs.LessonProgress;

public class CourseProgressDto
{
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public int TotalLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int ProgressPercent => TotalLessons == 0 ? 0 : (int)Math.Round((double)CompletedLessons / TotalLessons * 100);
    public List<LessonProgressDto> Lessons { get; init; } = new();
}
