namespace Lms.Api.DTOs.Enrollment;

public class EnrollmentDto
{
    public Guid Id { get; init; }
    public Guid CourseId { get; init; }
    public string CourseTitle { get; init; } = string.Empty;
    public DateTime EnrolledAt { get; init; }
    public string Status { get; init; } = string.Empty;
}
