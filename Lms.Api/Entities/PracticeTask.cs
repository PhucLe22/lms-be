namespace Lms.Api.Entities;

public class PracticeTask
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public Lesson Lesson { get; set; } = null!;
    public ICollection<PracticeSubmission> Submissions { get; set; } = new List<PracticeSubmission>();
}
