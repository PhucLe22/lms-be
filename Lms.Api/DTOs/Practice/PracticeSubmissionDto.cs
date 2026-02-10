namespace Lms.Api.DTOs.Practice;

public class PracticeSubmissionDto
{
    public Guid Id { get; init; }
    public Guid PracticeTaskId { get; init; }
    public string SubmissionType { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime SubmittedAt { get; init; }
    public string? StudentName { get; init; }
}
