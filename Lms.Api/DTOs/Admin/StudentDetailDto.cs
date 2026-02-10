namespace Lms.Api.DTOs.Admin;

public class StudentDetailDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public List<StudentEnrollmentDto> Enrollments { get; init; } = new();
}

public class StudentEnrollmentDto
{
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public int CompletedLessons { get; init; }
    public int TotalLessons { get; init; }
    public int ProgressPercent { get; init; }
}
