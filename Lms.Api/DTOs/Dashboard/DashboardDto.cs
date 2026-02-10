namespace Lms.Api.DTOs.Dashboard;

public class DashboardDto
{
    public int TotalEnrolledCourses { get; init; }
    public int CompletedCourses { get; init; }
    public int TotalLessonsCompleted { get; init; }
    public double OverallProgressPercent { get; init; }
    public double AverageQuizScore { get; init; }
    public int TotalQuizzesTaken { get; init; }
    public List<CourseProgressSummaryDto> Courses { get; init; } = new();
}

public class CourseProgressSummaryDto
{
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public int TotalLessons { get; init; }
    public int CompletedLessons { get; init; }
    public int ProgressPercent { get; init; }
}
