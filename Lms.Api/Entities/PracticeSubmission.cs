namespace Lms.Api.Entities;

public class PracticeSubmission
{
    public Guid Id { get; set; }
    public Guid PracticeTaskId { get; set; }
    public Guid UserId { get; set; }
    public string SubmissionType { get; set; } = "Text";
    public string Content { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }

    public PracticeTask PracticeTask { get; set; } = null!;
    public User User { get; set; } = null!;
}
