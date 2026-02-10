namespace Lms.Api.DTOs.Admin;

public class StudentListDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public int EnrolledCourses { get; init; }
}
